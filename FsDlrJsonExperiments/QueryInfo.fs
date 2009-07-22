

namespace Langexplr.Experiments

type QueryElement =
  | ElementQuery of string
  | SubElementQuery of string * string

module QueryInfo = begin

    let SplitWith (theString : string) (separator : string) =
       let rec Splitting (theString : string) (separator : string)  result =
          match theString.IndexOf(separator) with      
          | (-1) when theString.Length > 0 -> theString::result
          | i when i > 0 -> Splitting (theString.Substring(i+separator.Length))
                                      separator
                                      (theString.Substring(0,i)::result)
          | _ -> result
       List.rev( Splitting theString separator [])
       

      

    let ExtractQueryElements(name,alsoBySeparator,subQuerySeparator) =
       SplitWith name alsoBySeparator
        |> List.map (fun part -> 
                         match SplitWith part subQuerySeparator with
                         | [x] -> Some (ElementQuery x)
                         | [propertyName;subPropertyName] -> 
                                  Some (SubElementQuery(propertyName,subPropertyName))
                         | _ -> None)
        |> List.fold (fun l current -> 
                               match current with
                               | Some x -> x::l
                               | _ -> l) []
        |> List.rev
        
                      
       

    let GetQueryElements(methodName:string) =
       match methodName with
       | str when str.StartsWith("FindAllBy") ->
              Some(ExtractQueryElements(str.Substring("FindAllBy".Length),"AlsoBy","With")) 
       | str when str.StartsWith("find_all_by_") ->
           Some(ExtractQueryElements(str.Substring("find_all_by_".Length),"_also_by_","_with_") )
       | _ -> None
             
          
end 