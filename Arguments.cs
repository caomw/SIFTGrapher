using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace ConsoleApplication1
{
    class Arguments
    {

        [ValueOption(0)]
        public String DocumentRoot { get; set; }

        [Option('a', "area", DefaultValue = "areas.csv",
        HelpText = "Calculates the area under each curve and and outputs to the given file in CSV format.")]
        public String Area { get; set; }

        [Option('c', "clean", DefaultValue = false,
        HelpText = "Deletes all txt files after they are read.")]
        public bool Clean { get; set; }

        [OptionList('d', "desc", Separator = ',', HelpText = "A comma seperated list of descriptors to use.")]
        public IList<String> Descriptors { get; set; }

        [Option('f', "flip", DefaultValue = false,
        HelpText = "Graphs by descriptor instead of by image.")]
        public bool Flip { get; set; }

        [Option('g', "g", DefaultValue = 2,
        HelpText = "Number of graphs to put on a page. Min 1, max 3.")]
        public int Graphs { get; set; }

        [OptionList('i', "images", Separator = ',', HelpText = "A comma seperated list of image files to parse.")]
        public IList<String> Images { get; set; }

        [Option('o', "output",
        HelpText = "PDF file name [and location] to output to.")]
        public String Output { get; set; }

        [Option('s', "scale", DefaultValue = false,
        HelpText = "Scale individual graphs to best fit.")]
        public bool Scale { get; set; }

        [Option('t', "scaleglobal", DefaultValue = false,
        HelpText = "Scale to best global fit. (overrides -s)")]
        public bool GlobalScale { get; set; }

        [Option('v', "verbose", DefaultValue = false,
        HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        //[Option('r', "raw", DefaultValue = false,
        //HelpText = "Output raw text values at end of file.")]
        //public bool Raw { get; set; }


        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }

    }
}
