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
using System.Threading.Tasks;

using DocIntel.Core.Models;

namespace DocIntel.Core.Repositories
{
    public interface IImportRuleRepository
    {
        bool Exists(AmbientContext ambientContext, Guid importRuleId);
        bool SetExists(AmbientContext ambientContext, Guid importRuleSetId);

        Task<ImportRuleSet> Create(AmbientContext ambientContext, ImportRuleSet importRuleSet);
        Task<ImportRule> Create(AmbientContext ambientContext, ImportRule importRule);

        ImportRuleSet GetSet(AmbientContext ambientContext, Guid importRuleSetId);
        ImportRule Get(AmbientContext ambientContext, Guid importRuleId);

        IEnumerable<ImportRuleSet> GetAllSets(AmbientContext ambientContext);
        IEnumerable<ImportRule> GetAll(AmbientContext ambientContext, Guid setId);

        Task<ImportRuleSet> Update(AmbientContext ambientContext, ImportRuleSet importRuleSet);
        Task<ImportRule> Update(AmbientContext ambientContext, ImportRule importRule);

        Task RemoveSet(AmbientContext ambientContext, Guid importRuleSet);
        Task Remove(AmbientContext ambientContext, Guid importRule);
    }
}