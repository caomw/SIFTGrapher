using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Series;

namespace ConsoleApplication1
{
    public class ImagePlot
    {

        public PlotModel plot;
        public List<DescriptorData> descriptors;
        public String name;

        public ImagePlot(String n){
            name = n;
            descriptors = new List<DescriptorData>();
        }

    }
    
}
