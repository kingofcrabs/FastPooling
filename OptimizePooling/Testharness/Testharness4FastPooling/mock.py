from subprocess import call
def ScanBarcode( grid):
    "scan barcode for certain grid"
    scanResult = True;
    sBinFolder = "C:\\FastPooling\\bins\\";
    while True:
        call([sBinFolder + "SimulatePosID.exe",str(grid)]);
        call([sBinFolder + "Notifier.exe",str(grid)]); #notify main program to read barcode of grid x
        call([sBinFolder + "FeedMe.exe","FeedMe"]);
        resultFile = open("c:\\FastPooling\\Output\\result.txt");
        result = resultFile.read();
        resultFile.close();
        if result == "True":
            break;

def main():
    sBinFolder = "C:\\FastPooling\\bins\\";
    call([sBinFolder + "Notifier.exe","NewBatch"]);
    call([sBinFolder + "FeedMe.exe","FeedMe"]);
    f = open("C:\\FastPooling\\Output\\gridsCount.txt");
    gridNum = f.read();
    f.close();
    startGrid = 5;
    endGrid = startGrid + int(gridNum);
    for grid in range(startGrid,endGrid):
        ScanBarcode(grid);

main();