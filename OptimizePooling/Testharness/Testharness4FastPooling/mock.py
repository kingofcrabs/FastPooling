from subprocess import call
firstTimeError = True;
def ScanBarcode( grid):
    global firstTimeError    
    "scan barcode for certain grid"
    scanResult = True;
    sBinFolder = "C:\\FastPooling\\bins\\";
    while True:
        if( grid != 8):
            call([sBinFolder + "SimulatePosID.exe",str(grid)]);
        else :
            if firstTimeError:
                call([sBinFolder + "SimulatePosID.exe",str(grid)]);
                firstTimeError = False;
            else:
                call([sBinFolder + "SimulatePosID.exe",str(grid),"xx"]);

        call([sBinFolder + "Notifier.exe",str(grid)]); #notify main program to read barcode of grid x
        call([sBinFolder + "FeedMe.exe","FeedMe"]);
        #F:\\Projects\\FastPooling\\trunk\\OptimizePooling\\FastPooling\\bin\\Output\\result.txt
        #resultFile = open("c:\\FastPooling\\Output\\result.txt");
        resultFile = open("F:\\Projects\\FastPooling\\trunk\\OptimizePooling\\FastPooling\\bin\\Output\\result.txt");
        result = resultFile.read();
        resultFile.close();
        if result == "True":
            break;

def main():
    sBinFolder = "C:\\FastPooling\\bins\\";
    call([sBinFolder + "Notifier.exe","NewBatch"]);
    call([sBinFolder + "FeedMe.exe","FeedMe"]);
    #f = open("C:\\FastPooling\\Output\\gridsCount.txt");
    f = open("F:\\Projects\\FastPooling\\trunk\\OptimizePooling\\FastPooling\\bin\\Output\gridsCount.txt");
    gridNum = f.read();
    f.close();
    startGrid = 5;
    endGrid = startGrid + int(gridNum);
    for grid in range(startGrid,endGrid):
        ScanBarcode(grid);
    call([sBinFolder + "Notifier.exe","Gen"]);
main();
