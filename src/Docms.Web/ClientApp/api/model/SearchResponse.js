/**
 * DocMS API
 * Document Management System for small companies Web API
 *
 * OpenAPI spec version: v1
 * 
 *
 * NOTE: This class is auto generated by the swagger code generator program.
 * https://github.com/swagger-api/swagger-codegen.git
 * Do not edit the class manually.
 *
 */


import ApiClient from '../ApiClient';
import DocumentResponse from './DocumentResponse';
import TagResponse from './TagResponse';





/**
* The SearchResponse model module.
* @module model/SearchResponse
* @version v1
*/
export default class SearchResponse {
    /**
    * Constructs a new <code>SearchResponse</code>.
    * @alias module:model/SearchResponse
    * @class
    */

    constructor() {
        

        
        

        

        
    }

    /**
    * Constructs a <code>SearchResponse</code> from a plain JavaScript object, optionally creating a new instance.
    * Copies all relevant properties from <code>data</code> to <code>obj</code> if supplied or a new instance if not.
    * @param {Object} data The plain JavaScript object bearing properties of interest.
    * @param {module:model/SearchResponse} obj Optional instance to populate.
    * @return {module:model/SearchResponse} The populated <code>SearchResponse</code> instance.
    */
    static constructFromObject(data, obj) {
        if (data) {
            obj = obj || new SearchResponse();

            
            
            

            if (data.hasOwnProperty('searchTags')) {
                obj['searchTags'] = ApiClient.convertToType(data['searchTags'], [TagResponse]);
            }
            if (data.hasOwnProperty('documents')) {
                obj['documents'] = ApiClient.convertToType(data['documents'], [DocumentResponse]);
            }
        }
        return obj;
    }

    /**
    * @member {Array.<module:model/TagResponse>} searchTags
    */
    searchTags = undefined;
    /**
    * @member {Array.<module:model/DocumentResponse>} documents
    */
    documents = undefined;








}


