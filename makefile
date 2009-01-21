CC=gmcs
CVTEST_SRC = Emgu.CV.Test/*.cs  
SVN_URL = https://emgucv.svn.sourceforge.net/svnroot/emgucv/trunk/
VERSION = 1.5.0.0
VS2005_FOLDER=Solution/VS2005_MonoDevelop/
VS2008_FOLDER=Solution/VS2008/
CV_DLLS=cv110.dll cxcore110.dll cvaux110.dll highgui110.dll cxts001.dll ffopencv110.dll opencv.license.txt
LIB_DLLS=zlib.net.dll zlib.net.license.txt ZedGraph.dll ZedGraph.license.txt
FILE_TO_COPY=README.txt ${VS2005_FOLDER}Emgu.CV.sln ${VS2008_FOLDER}Emgu.CV.sln ${VS2005_FOLDER}Emgu.CV.Example.sln ${VS2008_FOLDER}Emgu.CV.Example.sln
CV_CHECKOUT=Emgu.CV Emgu.Util Emgu.CV.UI Emgu.CV.Example  

CV_RELEASE: CV.UI FORCE
	install -d release 
	cp Emgu.CV/README.txt Emgu.CV/Emgu.CV.License.txt bin/Emgu.CV.dll bin/Emgu.CV.UI.dll bin/Emgu.Util.dll release 
	$(foreach dll, $(LIB_DLLS), cp lib/$(dll) release;)
	tar -cv release | gzip -c > Emgu.CV.Linux.Binary-${VERSION}.tar.gz
	rm -rf release

Util:  FORCE  
	make -C Emgu.$@ bin; 

CV: Util FORCE 
	make -C Emgu.$@ bin;

CV.UI: CV FORCE
	make -C Emgu.$@ bin;

UI: 	FORCE
	make -C Emgu.$@ bin; 

CV_SRC:
	install -d src 
	install -d src/${VS2005_FOLDER}
	install -d src/${VS2008_FOLDER}
	install -d src/lib
	install -d src/bin
	$(foreach dll, ${LIB_DLLS}, cp lib/${dll} src/lib/;)
	$(foreach dll, ${CV_DLLS}, cp lib/${dll} src/bin/;)
	$(foreach folder, ${CV_CHECKOUT}, svn export ${SVN_URL}${folder} src/${folder};)
	$(foreach file, ${FILE_TO_COPY}, cp ${file} src/${file};)
	zip -r Emgu.CV.SourceAndExamples-${VERSION}.zip src
	rm -rf src

CVTest: CV UI $(CVTEST_SRC)
	$(CC) -target:library -r:System.Data -r:nunit.framework -r:bin/Emgu.Util.dll -r:bin/Emgu.UI.dll -r:System.Windows.Forms -r:System.Drawing -r:bin/Emgu.CV.dll $(CVTEST_SRC) -out:bin/Emgu.CV.Test.dll 

Test: CVTest
	cd bin; nunit-console2 Emgu.CV.Test.dll; cd ..

FORCE:
