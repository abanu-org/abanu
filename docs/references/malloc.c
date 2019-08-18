// https://raw.githubusercontent.com/bedrinejl/MicroKernel/master/libc/stdlib/malloc.c

/*
 * File: malloc.c
 * Author: Julien Freche <julien.freche@lse.epita.fr>
 *
 * Description: Simple and portable malloc implementation
 *
 */

#include <stdlib.h>
#include <sys/mman.h> // For mmap and munmap
#include <string.h> // For memcpy and memset
#include <unistd.h>

#ifdef THREAD_SAFE
# include <pthread.h>
pthread_mutex_t malloc_mutex = PTHREAD_MUTEX_INITIALIZER;
# define MALLOC_LOCK pthread_mutex_lock(&malloc_mutex);
# define MALLOC_UNLOCK pthread_mutex_unlock(&malloc_mutex);
#else
# define MALLOC_LOCK
# define MALLOC_UNLOCK
#endif

#define MIN(A, B) ((A) > (B) ? (B) : (A))

#define PAGE_SHIFT 12
#define PAGE_SIZE (1 << PAGE_SHIFT)
#define MIN_SHIFT 5
#define MIN_SIZE (1 << MIN_SHIFT)

struct malloc_meta;

struct malloc_list
{
  struct malloc_meta* next;
  struct malloc_meta* prev;
};

union malloc_data
{
  struct malloc_list free;
  char user[0];
};

// This struct has to be aligned on a machine word
struct malloc_meta
{
  size_t size;
  union malloc_data data;
};

typedef struct malloc_meta* pmeta;

pmeta list_heads[PAGE_SHIFT - 1];

#define META_SIZE (sizeof (size_t))

#define MAX_SHIFT_BIT ((size_t)1 << ((sizeof (size_t) * 8) - 1))
#define SET_INUSE(P) (P->size &= ~MAX_SHIFT_BIT)
#define SET_FREE(P) (P->size |= MAX_SHIFT_BIT)
#define IS_FREE(P) (P->size & MAX_SHIFT_BIT)
#define GET_SIZE(P) (P->size & ~MAX_SHIFT_BIT)
#define SET_SIZE(P, NSIZE) (P->size = IS_FREE(P) | (NSIZE))

#define MALLOC_MAX_SIZE (~MAX_SHIFT_BIT)

static void malloc_abort(const char* s)
{
  write(BW_COLOR, s, strlen(s));
}

__attribute__((const)) static size_t order(size_t l)
{
  size_t order = 0;
  size_t current = 1;

  if (!l)
    return 0;

  while ((l & current) == 0)
  {
    current <<= 1;
    order++;
  }

  return order;
}

__attribute__((const)) static size_t round_up_binary(size_t n)
{
  char   res = 0;
  size_t cur = 1;

  while (n > (cur << res))
    res++;

  return (res);
}

__attribute__((const)) static size_t size_to_page_number(size_t size)
{
  return ((size - 1) / PAGE_SIZE) + 1;
}

static void add_to_free_list(pmeta new)
{
  pmeta* list_head = &list_heads[order(GET_SIZE(new)) - MIN_SHIFT];

  SET_FREE(new);
  new->data.free.next = *list_head;
  new->data.free.prev = NULL;
  if (*list_head)
    (*list_head)->data.free.prev = new;
  *list_head = new;
}

static void remove_from_free_list(pmeta to_del)
{
  pmeta* list_head = &list_heads[order(GET_SIZE(to_del)) - MIN_SHIFT];

  SET_INUSE(to_del);
  if (*list_head == to_del)
    *list_head = to_del->data.free.next;
  if (to_del->data.free.next)
    to_del->data.free.next->data.free.prev = to_del->data.free.prev;
  if (to_del->data.free.prev)
    to_del->data.free.prev->data.free.next = to_del->data.free.next;
}

static pmeta create_meta(void* p, size_t l)
{
  pmeta meta = (pmeta)p;

  SET_SIZE(meta, l);
  SET_INUSE(meta);
  return meta;
}

static void create_buddy(pmeta b)
{
  size_t buddy = (size_t)b;
  size_t size = GET_SIZE(b);

  size >>= 1;
  SET_SIZE(b, size);
  buddy += size;
  pmeta buddy_meta = (pmeta)buddy;
  SET_SIZE(buddy_meta, size);
  add_to_free_list(buddy_meta);
}

static pmeta find_buddy(pmeta s)
{
  size_t addr = (size_t)s;
  addr ^= GET_SIZE(s);

  return (pmeta)(addr);
}

static pmeta get_meta(void* ptr)
{
  char* cptr = ptr;
  return (pmeta)(cptr - META_SIZE);
}

static pmeta split(pmeta b, size_t s)
{
  if (GET_SIZE(b) == s || GET_SIZE(b) == MIN_SIZE)
    return b;

  create_buddy(b);
  return split(b, s);
}

static pmeta fusion(pmeta b, size_t s)
{
  if (GET_SIZE(b) >= PAGE_SIZE || GET_SIZE(b) == s)
    return b;

  pmeta buddy = find_buddy(b);
  if (!IS_FREE(buddy) || GET_SIZE(buddy) != GET_SIZE(b))
    return b;

  remove_from_free_list(buddy);
  pmeta left_buddy = MIN(b, buddy);
  SET_SIZE(left_buddy, GET_SIZE(left_buddy) << 1);
  return fusion(left_buddy, s);
}

static void* large_malloc(size_t size)
{
  void* ptr = mmap(0, PROT_READ | PROT_WRITE, 1 + ((size - 1) / PAGE_SIZE));
  if (ptr == MAP_FAILED)
  {
    malloc_abort("[MALLOC] MMAP ERROR\n");
    return NULL;
  }
  pmeta meta = create_meta(ptr, size);

  return meta->data.user;
}

static pmeta find_free_block(size_t size)
{
  size_t current = order(size) - MIN_SHIFT;
  size_t max = PAGE_SHIFT - MIN_SHIFT;

  for (; current < max; current++)
  {
    pmeta block = list_heads[current];
    if (block)
    {
      remove_from_free_list(block);
      return block;
    }
  }

  return NULL;
}

static size_t internal_size(size_t size)
{
  size += META_SIZE;

  if (size >= PAGE_SIZE)
    return size_to_page_number(size) * PAGE_SIZE;

  if (size < MIN_SIZE)
    size = MIN_SIZE;
  size = 1 << round_up_binary(size);
  return size;
}

static void* buddy_malloc(size_t size)
{
  MALLOC_LOCK;
  pmeta block = find_free_block(size);
  if (block)
  {
    block = split(block, size);
    MALLOC_UNLOCK;
    return block->data.user;
  }

  void* ptr = mmap(0, PROT_READ | PROT_WRITE, 1);
  if (ptr == MAP_FAILED)
  {
    malloc_abort("[MALLOC] MMAP ERROR\n");
    return NULL;
  }

  pmeta meta = create_meta(ptr, PAGE_SIZE);
  meta = split(meta, size);
  MALLOC_UNLOCK;
  return meta->data.user;
}

void free(void* ptr)
{
  if (ptr == NULL)
    return;

  pmeta meta = get_meta(ptr);
  MALLOC_LOCK;
  meta = fusion(meta, PAGE_SIZE);
  if (GET_SIZE(meta) >= PAGE_SIZE)
  {
    MALLOC_UNLOCK;
    if (munmap(meta) != 0)
    {
      malloc_abort("[FREE] MUNMAP ERROR\n");
    }
    return;
  }

  add_to_free_list(meta);
  MALLOC_UNLOCK;
}

void* malloc(size_t size)
{
  if (!size)
    return NULL;

  void* ptr = NULL;
  size = internal_size(size);

  if (size > MALLOC_MAX_SIZE)
    return NULL;
  if (size > PAGE_SIZE)
  {
    ptr = large_malloc(size);
    return ptr;
  }

  ptr = buddy_malloc(size);
  return ptr;
}

void* realloc(void* ptr, size_t size)
{
  if (!ptr)
    return malloc(size);
  if (!size)
  {
    free(ptr);
    return NULL;
  }

  size_t user_size = size;
  size = internal_size(size);
  pmeta meta = get_meta(ptr);

  if (size > MALLOC_MAX_SIZE)
    return NULL;

  if (size <= PAGE_SIZE)
  {
    if (size == GET_SIZE(meta))
      return ptr;
    else if (size > GET_SIZE(meta))
    {
      size_t previous_size = GET_SIZE(meta) - META_SIZE;

      MALLOC_LOCK;
      meta = fusion(meta, size);
      if (GET_SIZE(meta) == size)
      {
        SET_INUSE(meta);
        MALLOC_UNLOCK;
        if (ptr != meta->data.user)
          memcpy(meta->data.user, ptr, previous_size);
        return meta->data.user;
      }
      MALLOC_UNLOCK;
    }
    else
    {
      MALLOC_LOCK;
      meta = split(meta, size);
      MALLOC_UNLOCK;
      return meta->data.user;
    }
  }

  size = user_size;
  void* new_ptr = malloc(size);
  if (!new_ptr)
    return NULL;
  pmeta mmeta = get_meta(ptr);
  memcpy(new_ptr, ptr, MIN(size, GET_SIZE(mmeta) - META_SIZE));
  free(ptr);

  return new_ptr;
}

void* calloc(size_t nmenb, size_t size)
{
  size_t mem_size = nmenb * size;
  void* ptr = NULL;

  if (!mem_size)
    return NULL;

  ptr = malloc(mem_size);
  if (!ptr)
    return NULL;
  memset(ptr, 0, mem_size);
  return ptr;
}
