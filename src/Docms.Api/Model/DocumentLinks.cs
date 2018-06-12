using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace IO.Swagger.Model {

  /// <summary>
  /// 
  /// </summary>
  [DataContract]
  public class DocumentLinks {
    /// <summary>
    /// Gets or Sets File
    /// </summary>
    [DataMember(Name="file", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "file")]
    public Link File { get; set; }

    /// <summary>
    /// Gets or Sets Self
    /// </summary>
    [DataMember(Name="self", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "self")]
    public Link Self { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class DocumentLinks {\n");
      sb.Append("  File: ").Append(File).Append("\n");
      sb.Append("  Self: ").Append(Self).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

    /// <summary>
    /// Get the JSON string presentation of the object
    /// </summary>
    /// <returns>JSON string presentation of the object</returns>
    public string ToJson() {
      return JsonConvert.SerializeObject(this, Formatting.Indented);
    }

}
}
