
namespace Langexplr.Experiments

open System.Dynamic
open System.Reflection
open System.Linq.Expressions
open Newtonsoft.Json.Linq
open Newtonsoft.Json


type FSDynJObjectWrapper(theObject:JObject) =
   inherit DynamicObject() with
   override this.TryGetMember(binder : GetMemberBinder, result : obj byref ) =
      match theObject.[binder.Name] with
      | null -> false
      | :? JObject as aJObject -> 
               result <- FSDynJObjectWrapper(aJObject)
               true
      | theValue -> 
               result <- theValue
               true