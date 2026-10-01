using System;
using EncosyTower.IO;

namespace EncosyTower.Persistences
{
    public delegate string MakeFilePathFunc(
          RootPath rootPath
        , string subFolderName
        , string fileName
        , string fileExtension
        , Type persistType
    );
}
