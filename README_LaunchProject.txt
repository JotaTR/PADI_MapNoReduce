README: How to run project
1. Build the project (in either "Debug" or "Release" mode)
	
2. Launch each part/component:
	- Puppet Master:
		1- Go into PuppetMaster->bin->(Mode you built the project in) folder
		2- Shift + Right-click in the folder and open a Command Window
			2.1- You can also open a Command Window in other folders, and then navigate to the folder mentioned above
		3- Run "PuppetMaster #npm #id",
			in that "#npm" is the number of other running puppet masters and "#id" is the launched puppet master id (1, 2, 3, etc.,
				and NOT the port number 20001-29999