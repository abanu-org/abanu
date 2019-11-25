#include <stddef.h>
#include <stdio.h>

int a = 0xFF334455;

void main()
{
	char text[] = "Hello, world!\n";
	char text2[] = "<ABC>";

	// for this example let's ignore result of write
	// but you should really handle it
	// 1 is stdout file handle

	//my_write(1, text, sizeof(text) - 1);

	//my_exit(0);

	//   while(1) {

	//	printf("Hello, World!");

	// }

	//  printf("Hello, World!");

	//fopen("alphabet.txt","wt");

	printf(text2);
	printf(text);

	while (1)
	{
		//printf("Hello, World!");
		char c = getchar();
		if (c != -1)
		{
			printf("char: ");
			putchar(c);
			printf(".");
		}
	}
}
