using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestParser
{
    public interface ITestCaseParser
    {
        string FilePattern { get; }
        List<ParsedTest> ParseFile(string fileContent);
    }
}
