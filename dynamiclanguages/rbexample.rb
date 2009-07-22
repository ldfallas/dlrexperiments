require 'FsDlrJsonExperiments.dll'
include Langexplr::Experiments

while true do
   print "Another try\n"
   str = System::Net::WebClient.new().download_string("http://twitter.com/statuses/public_timeline.json")

   #json = FSDynArrayWrapper.CreateFromFile('c:\\temp\\public_timeline.json')
  json = FSDynArrayWrapper.CreateFromReader(System::IO::StringReader.new(str))

  for i in json.find_all_by_user_with_time_zone('Central America') do
     print i.to_string()
  end
  sleep(5)
end
