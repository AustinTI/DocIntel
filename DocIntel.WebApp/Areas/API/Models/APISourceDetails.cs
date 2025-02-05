/* DocIntel
 * Copyright (C) 2018-2021 Belgian Defense, Antoine Cailliau
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;

using DocIntel.Core.Models;
using Newtonsoft.Json;

namespace DocIntel.WebApp.Areas.API.Models;

public class ApiSourceDetails : ApiSource
{
    /// <summary>
    /// The creation date
    /// </summary>
    [JsonProperty("creation_date")] 
    public DateTime CreationDate { get; set; }
    
    /// <summary>
    /// The last modification date
    /// </summary>
    [JsonProperty("modification_date")] 
    public DateTime ModificationDate { get; set; }
    
    /// <summary>
    /// The user who registered the source
    /// </summary>
    [JsonProperty("registered_by")] 
    public APIAppUser RegisteredBy { get; set; }
    
    /// <summary>
    /// The user who last modified the source
    /// </summary>
    [JsonProperty("last_modified_by")] 
    public APIAppUser LastModifiedBy { get; set; }
}