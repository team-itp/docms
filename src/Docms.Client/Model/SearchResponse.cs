using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Docms.Client.Model {

  /// <summary>
  /// 
  /// </summary>
  [DataContract]
  public class SearchResponse {
    /// <summary>
    /// Gets or Sets SearchTags
    /// </summary>
    [DataMember(Name="searchTags", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "searchTags")]
    public List<TagResponse> SearchTags { get; set; }

    /// <summary>
    /// Gets or Sets Documents
    /// </summary>
    [DataMember(Name="documents", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "documents")]
    public List<DocumentResponse> Documents { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class SearchResponse {\n");
      sb.Append("  SearchTags: ").Append(SearchTags).Append("\n");
      sb.Append("  Documents: ").Append(Documents).Append("\n");
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
