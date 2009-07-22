import clr

clr.AddReference("Newtonsoft.Json")
clr.AddReference("FsDlrJsonExperiments.dll")

from System.IO import StreamReader
from Newtonsoft.Json import *
from Newtonsoft.Json.Linq import *
from Langexplr.Experiments import *

fReader = StreamReader('c:\\temp\\public_timeline.json')
jReader  = JsonTextReader(fReader)
serializer = JsonSerializer()

json = FSDynArrayWrapper( serializer.Deserialize(jReader) )

for i in json.FindAllByFavoritedAlsoByUserWithScreen_Name("false","bnmeeks"):
   print i


jReader.Close()
fReader.Close()
