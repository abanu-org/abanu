// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

namespace Abanu.Kernel
{
    public interface ITextConsoleDevice
    {
        int Rows { get; }
        int Columns { get; }

        void SetChar(int row, int column, ConsoleChar c);

        unsafe void SetChar(int offset, ConsoleChar c);

        void SetCursor(int row, int col);
    }

}
