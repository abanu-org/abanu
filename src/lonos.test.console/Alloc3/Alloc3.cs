using System;
namespace Lonos.test.console.Alloc3
{

    public struct page
    {
    }

    public unsafe struct LinkedListNode
    {
        public LinkedListNode* Prev;
        public LinkedListNode* Next;
        public void* Data;
    }

    public unsafe struct LinkedList
    {
        public LinkedListNode* FirstNode;
        public LinkedListNode* LastNode;

        public void Remove(LinkedListNode* node)
        {
            if (node->Prev == null)
                FirstNode = node->Next;
            else
                node->Prev->Next = node->Next;
            if (node->Next == null)
                LastNode = node->Prev;
            else
                node->Next->Prev = node->Prev;
        }

        public void InsertAfter(LinkedListNode* node, LinkedListNode* newNode)
        {
            newNode->Prev = node;
            if (node->Next == null)
            {
                newNode->Next = null; //(not always necessary)
                LastNode = newNode;
            }
            else
            {
                newNode->Next = node->Next;
                node->Next->Prev = newNode;
            }
            node->Next = newNode;
        }

        public void InsertBefore(LinkedListNode* node, LinkedListNode* newNode)
        {
            newNode->Next = node;
            if (node->Prev == null)
            {
                newNode->Prev = null; // (not always necessary)
                FirstNode = newNode;
            }
            else
            {
                newNode->Prev = node->Prev;
                node->Prev->Next = newNode;
            }
            node->Prev = newNode;
        }

        public void InsertBeginning(LinkedListNode* newNode)
        {
            if (FirstNode == null)
            {
                FirstNode = newNode;
                LastNode = newNode;
                newNode->Prev = null;
                newNode->Next = null;
            }
            else
                InsertBefore(FirstNode, newNode);
        }


        public void InsertEnd(LinkedListNode* newNode)
        {
            if (LastNode == null)
                InsertBeginning(newNode);
            else
                InsertAfter(LastNode, newNode);
        }
    }

    public class Alloc3
    {
        public Alloc3()
        {
        }
    }
}
