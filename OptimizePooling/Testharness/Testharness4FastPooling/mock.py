from subprocess import call



def ScanBarcode( grid):
    "scan barcode for certain grid"
    scanResult = True;
    while True:
        call(["F:\\Projects\\FastPooling\\trunk\\OptimizePooling\\SimulatePosID\\bin\\Debug\\SimulatePosID.exe",str(grid)]);
        call(["C:\\Notifier\\Notifier.exe",str(grid)]); #notify main program to read barcode of grid x
        call(["C:\\FeedMe\\FeedMe.exe","Feed"]);
        resultFile = open("F:\\Projects\\FastPooling\\trunk\\OptimizePooling\\FastPooling\\bin\\Output\\result.txt");
        result = resultFile.read();
        resultFile.close();
        if result == "True":
            break;
        

def main():
    call(["C:\\Notifier\\Notifier.exe","NewBatch"]);
    call(["C:\\FeedMe\\FeedMe.exe","Feed"]);
    f = open("F:\\Projects\\FastPooling\\trunk\\OptimizePooling\\FastPooling\\bin\\Output\\gridsCount.txt");
    gridNum = f.read();
    f.close();
    startGrid = 5;
    endGrid = startGrid + int(gridNum);
    for grid in range(startGrid,endGrid):
        ScanBarcode(grid);
    

main();