# SIFTGrapher

This project aims to make graphing the results of SIFT operations from the OpenCV library easy and quick.

#### Synopsis
```
SIFTGrapher.exe <images_directory> [-cfstv] [-a <area_output>] [-d <descriptor_list>] [-g <number_graphs>] [-i <image_list] [-o <graph_output>]  [--help]
```
The only reqirement is that the directory containing the images is passed as the first argument. -o and -a should be used to produce meaningful output.

#### Command Line Arguments

##### -a, --area
Output a CSV file with the integrated areas under each curve.
##### -c, --clean
Removes all used txt files after they are read.
##### -d, --desc
A comma seperated list of descriptors to limit the files that will be processed. For example ` -d SIFT,GreyTexSIFT,GreyTexCircSIFT` will only process files matching SIFT, GreyTexSIFT, and GreyTexCircSIFT.
##### -f, --flip
By default, graphs and outputs are by image. This option graphs and outputs data by descriptor.
##### -g, --g
The number of graphs to fit onto a page. The default is 2. Min 1, max 3.
##### -i, --i
A comma seperated list of images to limit the image folders that will be processed. For example ` -i trees,graf,wall` will only process files matching trees, graf, and wall.
##### -o, --output
Output a PDF file with the graphs for each descriptor.
##### -s, --scale
Scales each graph to best fit.
##### -t, --scaleglobal
Scales each graph to the best global fit. Each graph will have the same scale. This overrides the -s argument.
##### -v, --verbose
Prints progress and detail messages.
##### --help
Displays a help screen.
