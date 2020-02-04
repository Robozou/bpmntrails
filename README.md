# bpmntrails

This project is made in order to convert DCR graph simulation traces into BPMN graphs.

The project is independent and relies on no third party tools.

To use the program:
Download and build the project.
In "bpmntrails\Tester\bin\Debug" find the file Tester.exe.
Execute Tester.exe with the arguments: "location/of/output/filename.xml" "location/of/graph/filename.xml" "location/of/traces/filename.xml".
Find the newly created BPMN graph in the specified location.

The "Getter" project in the solution is a tool to access the WEB Api on DCR Solutions - this is a paid service.

In order to use the Getter add a new file "login.txt" in the directory one level above the .git folder.
In this file on the first line write your username for DCRgraphs.net, and on the second line write your password.
When using the Getter you can specify the output location of your graph and its traces by changing the "GraphLoc" and "TraceLoc" respectively.
You specify the graph by specifying the graph id in the "testgraph" variable.
