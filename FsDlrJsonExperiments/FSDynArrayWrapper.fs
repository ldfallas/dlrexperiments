
namespace Langexplr.Experiments

open System.Dynamic
open System.Reflection
open System.Linq.Expressions
open Newtonsoft.Json.Linq
open Newtonsoft.Json

module ExprHelper = begin
   let rec CompareHelper(o : obj, s : string) =
     match o with
     | :? string as e1 -> 
             System.String.Compare( e1,s,true) = 0  
     | :? JValue as literalValue -> 
             CompareHelper(literalValue.Value,s)     
     | null -> false        
     | other -> CompareHelper(other.ToString(),s)
     
  let GetJObjectPropertyCI(o:JObject ,propertyName) =
     let property = 
      Seq.tryFind 
          (fun (aProperty:JProperty) -> 
                System.String.Compare(aProperty.Name,
                                      propertyName,
                                      true) = 0 ) 
          (o.Properties()) in
     match property with
     | Some theMatch -> theMatch.Value
     | None -> JValue(null :> obj) :> JToken
end


type FSDynArrayWrapperMetaObject(expression : Expression, value: System.Object) = 
   inherit DynamicMetaObject(expression,BindingRestrictions.Empty,value) 
   
   let getJsonPropertyMethodInfo = Assembly.GetExecutingAssembly().GetType("Langexplr.Experiments.ExprHelper").GetMethod("GetJObjectPropertyCI")
   let compareHelperMethodInfo = Assembly.GetExecutingAssembly().GetType("Langexplr.Experiments.ExprHelper").GetMethod("CompareHelper")
   
   member this.GetJObjectPropertyExpression(expression: Expression,propertyName:string) =
      Expression.Call(
         getJsonPropertyMethodInfo,
         Expression.Convert(expression,typeof<JObject>),
         Expression.Constant(propertyName)) :> Expression
   
   member this.GetPropertyExpressionForQueryArgument(parameter:QueryElement,argument,cParam,tmpVar) : Expression =
             match parameter with
             | ElementQuery(propertyName) -> 
                 this.CompareExpression(     
                    this.GetJObjectPropertyExpression(cParam,propertyName) ,
                    argument) 
             | SubElementQuery(propertyName,subPropertyName) ->
                 Expression.And(
                   Expression.NotEqual(
                       Expression.TypeAs(
                          Expression.Assign(
                              tmpVar,
                              this.GetJObjectPropertyExpression(cParam,propertyName)),
                          typeof<JObject>),  
                       Expression.Constant(null)),          
                   this.CompareExpression(          
                       this.GetJObjectPropertyExpression(
                           tmpVar,
                           subPropertyName),argument)) :> Expression
                           
   member this.CompareExpression(exp1 : Expression,exp2 : Expression) : Expression =
     Expression.Call(
         compareHelperMethodInfo,
         [| exp1; exp2|] ) :> Expression
         
   member this.GenerateCodeForBinder(elements, arguments : Expression array) =
      let whereParameter = Expression.Parameter(typeof<JToken>, "c") in
      let tmpVar = Expression.Parameter(typeof<JToken>, "tmp") in
      let whereMethodInfo =
            (typeof<System.Linq.Enumerable>).GetMethods() 
                    |> Seq.filter (fun (m:MethodInfo) -> m.Name = "Where" && (m.GetParameters().Length = 2))
                    |> Seq.map (fun (m:MethodInfo) -> m.MakeGenericMethod(typeof<JToken>))
                    |> Seq.hd
      let queryElementsConditions =              
           elements 
              |> Seq.zip arguments
              |> Seq.map 
                     (fun (argument,queryParameter) ->
                          this.GetPropertyExpressionForQueryArgument(queryParameter,argument,whereParameter,tmpVar)) in
      let initialConditions = [ Expression.TypeIs(whereParameter,typeof<JObject>) ] in
      
      let resultingExpression =
           Expression.Block(
              [tmpVar],
              Expression.Call(
                 whereMethodInfo,
                 Expression.Property(
                    Expression.Convert(
                       this.Expression,this.LimitType),"array"),
                 Expression.Lambda(                     
                     Seq.fold 
                          (fun s c -> Expression.And(s,c) :> Expression) 
                          ((List.hd initialConditions) :> Expression) 
                          queryElementsConditions,
                      whereParameter))) in
      resultingExpression                
      
      
                    
   override this.BindGetMember(binder: GetMemberBinder) =
     match QueryInfo.GetQueryElements(binder.Name) with
     | Some( elements ) ->
         let parameters =
           List.mapi ( fun i _ ->
                            Expression.Parameter(
                                        typeof<string>,
                                        sprintf "p%d" i)) elements
         (new DynamicMetaObject(
             Expression.Lambda(
                 this.GenerateCodeForBinder(
                     elements, 
                     parameters 
                     |> List.map (fun p -> p :> Expression)
                     |> List.to_array ),
                 parameters),
             binder.FallbackGetMember(this).Restrictions))
     | None -> base.BindGetMember(binder)
   
   override this.BindInvokeMember(binder: InvokeMemberBinder,args: DynamicMetaObject array) =
     match QueryInfo.GetQueryElements(binder.Name) with
     | Some( elements ) ->
         (new DynamicMetaObject(
             this.GenerateCodeForBinder(
                 elements, 
                 Array.map 
                   (fun (v:DynamicMetaObject) -> 
                      Expression.Constant(v.Value.ToString()) :> Expression) args),
             binder.FallbackInvokeMember(this,args).Restrictions))
     | None -> base.BindInvokeMember(binder,args)
      
    
     
type FSDynArrayWrapper(a:JArray) =   
   member this.array with get() = a
   static member CreateFromReader(stream : System.IO.TextReader) =
     using (new JsonTextReader(stream)) 
           (fun (jr : JsonTextReader) ->
                let d = new JsonSerializer() in
                  FSDynArrayWrapper(d.Deserialize(jr) :?> JArray))
   static member CreateFromFile(fileName:string) =
     using (new System.IO.StreamReader(fileName)) 
           (fun (reader : System.IO.StreamReader) ->
                FSDynArrayWrapper.CreateFromReader(reader))
                  
   interface IDynamicMetaObjectProvider with
      member this.GetMetaObject( parameter : Expression) : DynamicMetaObject =
         FSDynArrayWrapperMetaObject(parameter,this)  :> DynamicMetaObject
         
  

   


   
   
   