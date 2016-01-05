using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CommandLine;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Reporting;
using OxyPlot.Pdf;
using OxyPlot.Axes;
using OxyPlot.Annotations;

namespace ConsoleApplication1
{

    public struct DescriptorData
    {

        //public DescriptorData(String descriptor, String image)
        //{
        //    totalMatches = 0;
        //    correctMatches = 0;
        //    this.descriptor = descriptor;
        //    this.image = image;
        //    pointData = new List<PointData>();
        //}

        public int totalMatches;
        public int correctMatches;
        public String descriptor;
        public String image;

        public struct PointData
        {
            public float correct;
            public float total;
            public float precision;
            public float recall;
        }

        public List<DescriptorData.PointData> pointData;

    }

    class Program
    {

        private static Arguments arguments;

        private static OxyColor[] colors;

        private static float globalScaleMax = 0.0f;


        static void Main(string[] args)
        {

            List<OxyColor> colorTemp = new List<OxyColor>();

            colorTemp.Add(OxyColor.FromRgb(255, 127, 14));
            colorTemp.Add(OxyColor.FromRgb(214, 39, 40));
            colorTemp.Add(OxyColor.FromRgb(148, 103, 189));
            colorTemp.Add(OxyColor.FromRgb(44, 160, 44));
            colorTemp.Add(OxyColor.FromRgb(31, 119, 180));
            colorTemp.Add(OxyColor.FromRgb(227, 119, 194));
            colorTemp.Add(OxyColor.FromRgb(188, 189, 34));
            colorTemp.Add(OxyColor.FromRgb(140, 86, 75));
            colorTemp.Add(OxyColor.FromRgb(127, 127, 127));
            colorTemp.Add(OxyColor.FromRgb(23, 190, 207));

            colors = colorTemp.ToArray();

            Dictionary<String, ImagePlot> graphs = new Dictionary<String, ImagePlot>();

            // Minimum command line argument is the path to a descriptor
            if (args.Length < 1)
            {
                Console.WriteLine("Must specify file path as first argument. Run with --help for usage insructions.");
                
                return;
            }

            // Parse the argument list
            arguments = new Arguments();
            if(!Parser.Default.ParseArguments(args, arguments))
            {
                Console.WriteLine("Illegal arguments. Run with --help for usage instructions.");
                
                return;
            }


            // If the directory exists, go to each image (or the images that are listed)
            if (Directory.Exists(arguments.DocumentRoot))
            {
                // Get a list of the subdirectories
                String[] subDirectories = Directory.GetDirectories(arguments.DocumentRoot);
                for(int i = 0; i < subDirectories.Length; i++)
                {
                    String imageName = Path.GetFileName(subDirectories[i]);

                    bool runImage = true;
                    if(arguments.Images != null)
                    {
                        runImage = false;
                        foreach(String s in arguments.Images)
                        {
                            if (s.Equals(imageName,StringComparison.InvariantCultureIgnoreCase)){
                                runImage = true;
                                break;
                            }
                        }
                    }

                    if(runImage)
                    {
                        if (arguments.Verbose) Console.WriteLine("Processing image: " + imageName);
                        DescriptorData[] datas = ProcessImage(imageName, subDirectories[i]);

                        // Add the datas to the graph objects
                        foreach(DescriptorData data in datas)
                        {
                            String key = data.image;
                            if(arguments.Flip)
                                key = data.descriptor;

                            if(!graphs.ContainsKey(key))
                            {
                                graphs.Add(key, new ImagePlot(key));
                            }

                            ImagePlot plot = graphs[key];
                            plot.descriptors.Add(data);

                        }
                    }
                    
                }

                // Graph the data
                foreach(var item in graphs)
                {
                    GenerateGraph(item.Value);
                }

                ProcessReport(graphs.Values.ToArray());

            }
            else
            {
                Console.WriteLine("Image diretory is invalid!");
                return;
            }

            // Make the report with the image data.
           // ProcessReport(datas.ToArray());

            if (arguments.Verbose)
                Console.WriteLine("Done!");

            Console.ReadLine();
        }

        static DescriptorData ReadFile(String filename, String descriptor, String imageName)
        {

            DescriptorData dscData = new DescriptorData();
            dscData.image = imageName;
            dscData.descriptor = descriptor;

            try
            {   // Open the text file using a stream reader.
                String[] lines = File.ReadAllLines(filename);

                if(lines.Length > 1)
                {

                    String[] values = lines[0].Split(null);
                    dscData.correctMatches = Int32.Parse(values[0]);
                    dscData.totalMatches = Int32.Parse(values[1]);

                    dscData.pointData = new List<DescriptorData.PointData>();

                    for(int i = 1; i < lines.Length; i++)
                    {

                        DescriptorData.PointData data = new DescriptorData.PointData();

                        values = lines[i].Split(null);
                        float correct = Int32.Parse(values[0]);
                        float total = Int32.Parse(values[1]);

                        data.correct = correct;
                        data.total = total;

                        dscData.pointData.Add(data);

                    }

                    for (int i = 0; i < dscData.pointData.Count; i++)
                    {

                        DescriptorData.PointData data = dscData.pointData[i];
                        
                        // First check if there are 0 correct matches
                        if(data.correct == 0)
                        {
                            data.recall = 0;
                            data.precision = 1;
                        }
                        else
                        {
                            // Calculate the precision and make sure
                            // that the toal matches are greater than 0, but they
                            // really always should be...
                            if(dscData.totalMatches > 0)
                            {
                                data.recall = data.correct / dscData.totalMatches;
                            }
                            else
                            {
                                data.recall = 1;
                            }

                            // Calculate the recall
                            if (data.total > 0)
                            {
                                data.precision = data.correct / data.total;
                            }
                            else
                            {
                                data.precision = 0;
                            }

                        }

                        globalScaleMax = Math.Max(globalScaleMax, data.recall);

                        dscData.pointData[i] = data;

                    }

                }
               
                
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read: ");
                Console.WriteLine(e.Message);
            }
            
            return dscData;

        }

        static void GenerateGraph(ImagePlot plot)
        {
            
            PlotModel model = new PlotModel { Title = plot.name };
            
            model.LegendTitle = "Descriptor";
            if (arguments.Flip)
                model.LegendTitle = "Image";

            float maxPrecision = 0;
            int colorIndex = 0;

            foreach(DescriptorData data in plot.descriptors)
            {

                String title = data.descriptor;
                if (arguments.Flip)
                    title = data.image;

                title += " (" + data.correctMatches + "/" + data.totalMatches + ")";

                
                LineSeries line = new LineSeries { Title = title, Color = colors[colorIndex++ % colors.Length] };
                for (int i = 0; i < data.pointData.Count; i++ )
                {
                    maxPrecision = Math.Max(maxPrecision, data.pointData[i].precision);
                    line.Points.Add(new DataPoint(data.pointData[i].precision, data.pointData[i].recall));
                }
                model.Series.Add(line);
            }

            // If global scale is on, we'll use that instead.
            if (arguments.GlobalScale)
                maxPrecision = globalScaleMax;

            // Add some padding
            maxPrecision = (float)Math.Min(maxPrecision + (maxPrecision * .2), 1.2f);

            // If we aren't scaling, just set it to 1.0
            if (!arguments.Scale && !arguments.GlobalScale)
                maxPrecision = 1.0f;

            LineAnnotation max = new LineAnnotation { Type = LineAnnotationType.Vertical, X = 1.0, Color = OxyColors.LightGray, LineStyle = OxyPlot.LineStyle.Dash };

            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = 0.0, Maximum = maxPrecision, Title = "Recall" });
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = 0.0, Maximum = 1.05, Title = "Precision" });
            model.Annotations.Add(max);

            plot.plot = model;

        }

        static void ProcessReport(ImagePlot[] imageData)
        {

            String fileName = "Output.pdf";

            if(arguments.Output != null)
            {
                fileName = arguments.Output;
            }

            try
            {
                var s = File.Create(fileName);
                var r = new Report();
                r.AddHeader(1, "Output generated: "+DateTime.Now);

                int numReports = arguments.Graphs;
                int height = 0;
                switch (numReports)
                {
                    case 1:
                        height = 800;
                        break;
                    case 2:
                        height = 600;
                        break;
                    default:
                        height = 400;
                        break;
                }

                foreach(ImagePlot data in imageData)
                {
                    r.AddPlot(data.plot, null, 800, height);
                }

                //if (arguments.Raw && false)
                //{
                //    //r.AddParagraph()
                //    r.AddParagraph("this is a test");
                //}

                using (var w = new PdfReportWriter(s))
                {
                    w.WriteReport(r, new ReportStyle {});
                    if (arguments.Verbose) Console.WriteLine("Output to: " + fileName);
                }
            } 
            catch (Exception e)
            {
                Console.WriteLine("Could not output to file: " + e.Message);
            }
            
            
        }

        // Get all of the descriptors for an image directory
        static DescriptorData[] ProcessImage(String imageName, String filename)
        {
            String[] files = Directory.GetFiles(filename);

            List<DescriptorData> datas = new List<DescriptorData>();

            for (int i = 0; i < files.Length; i++ )
            {
                String extension = Path.GetExtension(files[i]);
                String fileName = Path.GetFileName(files[i]);

                // See if it is an output image file
                if (extension == ".txt" && fileName.Contains("SIFT"))
                {

                    // Get the descriptor name
                    int uIndex = fileName.IndexOf('_');
                    if(uIndex > 0)
                    {
                        String descriptor = fileName.Substring(0,uIndex);

                        bool runDescriptor = true;
                        if(arguments.Descriptors != null)
                        {
                            runDescriptor = false;
                            foreach(String s in arguments.Descriptors)
                            {
                                if (s.Equals(descriptor,StringComparison.InvariantCultureIgnoreCase)){
                                    runDescriptor = true;
                                    break;
                                }
                            }
                        }

                        if (runDescriptor)
                        {

                            if (arguments.Verbose) Console.WriteLine("Processing descriptor: " + descriptor);
                            datas.Add(ReadFile(files[i], descriptor, imageName));
                        }
                    }

                    try
                    {
                        if (arguments.Clean)
                        {
                            File.Delete(files[i]);
                            if (arguments.Verbose) Console.WriteLine("Deleting: " + files[i]);
                        }
                           
                    }
                    catch (System.IO.IOException e)
                    {
                        Console.WriteLine("Could not delete file: "+e.Message);
                    }
                }
            }

            return datas.ToArray();

        }

    }
}
 