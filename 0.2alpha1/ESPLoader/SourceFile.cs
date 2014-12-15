using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ESPLoader
{

    class SourceFile
    {
        public string Filepath { get; private set; }
        public string ShortName { get; private set; }
        public int MemoryLocation { get; private set; }
        public byte[] Contents { get; private set; }
        public bool isValid { get; private set; }

        public SourceFile(string filepath, int memorylocation)
        {
            Filepath = filepath;
            MemoryLocation = memorylocation;

            ShortName = Path.GetFileNameWithoutExtension(Filepath);

            Refresh();
        }

        public void Refresh()
        {
            try
            {
                Contents = File.ReadAllBytes(Filepath);
            }
            catch
            {
                isValid = false;
            }

            isValid = true;
        }
    }
}
