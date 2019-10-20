// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

namespace Lonos.Kernel
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
