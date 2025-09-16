using AngleSharp.Text;
using Castle.Core.Resource;
using Dapper.Oracle;
using GTM_Admin_API.DataResourceUpdate;
using GTM_Admin_API.Extensions;
using GTM_Admin_API.Options;
using GTMData.Application.Constants;
using GTMData.Application.Extensions;
using GTMData.Application.Interfaces;
using GTMData.Application.Parameters;
using GTMData.Core.Entities;
using GTMData.Infrastructure.Data;
using GTMData.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using static IdentityModel.ClaimComparer;
using static Microsoft.AspNetCore.Http.StatusCodes;

using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GTM_Admin_API.Controllers
{

    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]

    public class SecurityResouresController : APIControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IDbConnection dbConnection;
        private readonly ICurrentUserService _currentUser;
        private readonly IMemoryCache _memoryCache;
        private IDistributedCache _redisCache { get; set; }
        private readonly SecurityResourcesOptions_v1 _ver1;
        private readonly ILogger<SecurityResouresController> _logger;
        private IConfiguration _config;

        public class policyRoles
        {
            public IEnumerable<dynamic> Data { get; set; }

        }

        //public class securityRoles
        //{
        //    public IEnumerable<userRole> Data { get; set; }

        //}
        public class securityResources
        {
            public ResourceCollection Data { get; set; }
        }

        public class ResourceCollection
        {
            public List<string> Resources { get; set; }
        }

        public class resourceNames
        {
            public IEnumerable<dynamic> Data { get; set; }


        }

        public class securityRoles
        {
            public IEnumerable<userRole> Data { get; set; }

        }

            public class columnNames
        {
            public IEnumerable<dynamic> Data { get; set; }


        }
        public class userRole
        {
            public string roleCode { get; set; }

        }

        public class ResourceName
        {
            public string resource { get; set; }

        }

        //Get all Policies with the Roles
        public SecurityResouresController(IUnitOfWork unitOfWork, IDbConnection dbConnection, ICurrentUserService currentUser, IDistributedCache redisCache,
            IOptions<SecurityResourcesOptions_v1> apiVer1, ILogger<SecurityResouresController> logger, IConfiguration config)
        {
            this.unitOfWork = unitOfWork;
            this._currentUser = currentUser;
            this._redisCache = redisCache;
            this._ver1 = apiVer1.Value;
            _logger = logger;
            _config = config;
            this.dbConnection = dbConnection;

        }


        public class ColumnResources
        {
            [JsonInclude]
            public IEnumerable<ResourceColumnAccess> Data;
        }




        public class PolicyRolesBody
        {
            [JsonPropertyName("RoleCode")]
            [JsonInclude]
            public string RLCD { get; set; }
            [JsonPropertyName("PolicyKey")]
            [JsonInclude]
            public string APKEY { get; set; }

        }

        public class PolicyRolesResource
        {

            public string RLCD { get; set; }
            public string APKEY { get; set; }
            public string APRID { get; set; }

        }

        public class PolicyResource //polcy
        {
            public string APKEY { get; set; }
            // public string APCREATEID { get; set; }

        }




        [HttpDelete]
        [MapToApiVersion("1.0")]
        [Authorize(Policy = Policies.IsAllUserRoles)]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("PolicyRoles/")]
        public async Task<IActionResult> DeletePolicyWithRole([FromBody] PolicyRolesBody policyRoles)
        {
            string entityId = "";
            int errorId = 0;
            string userId = "";

            if (!string.IsNullOrEmpty(_currentUser.AdAccountName))
            {
                userId = _currentUser.AdAccountName;
            }
            else
            {
#if DEBUG
                userId = "dosdev\\shehzadn2"; // hardcoded for testing only
#endif
            }

            PolicyRolesResource policyRolesRes = new PolicyRolesResource
            {
                APKEY = policyRoles.APKEY,
                RLCD = policyRoles.RLCD

            };

            var optionPolicyRoleRes = new JsonSerializerOptions
            {
                Converters =
                {
                    new BasePropertyNameConverter<PolicyRolesResource>()
                }
            };

            /******************************************************************/
            //this creates Logic for Policy role


            string procNameRole = "SECREF.pkg_webapi_POLICIES_DELETE_V01.prc_DELETE_apr"; //policy only
            //Logic for Policy insert first
            string jsonPolicyRolesString = JsonSerializer.Serialize(policyRolesRes, optionPolicyRoleRes).Insert(0, "{\"Data\":");
            jsonPolicyRolesString = jsonPolicyRolesString.Insert(jsonPolicyRolesString.Length, "}");

            //Input params
            OracleDynamicParameters oracleDynamicParameters = base.SetDBExecutionParamsDelete(jsonPolicyRolesString, userId, "GTMData.DeetePolicyWithRole.Delete.v01");

            var result = await unitOfWork.TemplatesRepository.GenericDBCallAsync(
                oracleDynamicParameters, procNameRole);

            if (oracleDynamicParameters.ParameterNames.Contains("pv_wl_id_o"))
                entityId = oracleDynamicParameters.Get<string>("pv_wl_id_o");

            if (result.ReturnCode < 0)
                errorId = result.ReturnCode;


            //this will delete if no more roles left
            PolicyResource policyRes = new PolicyResource
            {
                APKEY = policyRoles.APKEY,

            };

            var optionPolicyeRes = new JsonSerializerOptions
            {
                Converters =
                {
                    new BasePropertyNameConverter<PolicyResource>()
                }
            };

            //this create policy record only if it doesn't exist
            string jsonPolicyString = JsonSerializer.Serialize(policyRes, optionPolicyeRes).Insert(0, "{\"Data\":");
            jsonPolicyString = jsonPolicyString.Insert(jsonPolicyString.Length, "}");
            string procNameRolePolicy = "SECREF.pkg_webapi_POLICIES_DELETE_V01.prc_DELETE_ap"; //policy and role
            OracleDynamicParameters oracleDynamicParametersPolicy = base.SetDBExecutionParamsDelete(jsonPolicyString, userId, "GTMData.DeletePolicy.Delete.v01");

            var result1 = await unitOfWork.TemplatesRepository.GenericDBCallAsync(
                oracleDynamicParametersPolicy, procNameRolePolicy);

            if (oracleDynamicParametersPolicy.ParameterNames.Contains("pv_wl_id_o"))
                entityId = oracleDynamicParametersPolicy.Get<string>("pv_wl_id_o");


            // if (result1.ReturnCode < 0) policy already exists, but should move forward to add role policy combination
            if (result1.ReturnCode == 0)
            {
                ((CommonOutcomes.Success<IEnumerable<object>>)result1).Data = new List<string> { $"Role: {policyRoles.RLCD} with policy key: {policyRoles.APKEY} has been deleted successfully." };
            }
            else

                ((CommonOutcomes.Success<IEnumerable<object>>)result).Data = new List<string> { $"There was an issue deleting {policyRoles.RLCD} Or policy key: {policyRoles.APKEY} " };

            return Ok(result1);

        }

        [HttpPost]
        [MapToApiVersion("1.0")]
         [Authorize(Policy = Policies.IsAllUserRoles)]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Route("PolicyRoles/")]
        public async Task<IActionResult> AddPolicyWithRole([FromBody] PolicyRolesBody policyRoles)
        {
            var optionsPolicyRoleResource = new JsonSerializerOptions
            {
                Converters =
                {
                    new BasePropertyNameConverter<PolicyRolesResource>()
                }
            };

            string entityId = "";
            int errorId = 0;
            string userId = "";

            if (!string.IsNullOrEmpty(_currentUser.AdAccountName))
            {
                userId = _currentUser.AdAccountName;
            }
            else
            {
#if DEBUG
                userId = "dosdev\\shehzadn2"; // hardcoded for testing only
#endif
            }

            PolicyResource policyRes = new PolicyResource
            {
                APKEY = policyRoles.APKEY,

            };

            var optionPolicyeRes = new JsonSerializerOptions
            {
                Converters =
                {
                    new BasePropertyNameConverter<PolicyResource>()
                }
            };

            //Part 1 -  create policy record only if it doesn't exist
            string jsonPolicyString = JsonSerializer.Serialize(policyRes, optionPolicyeRes).Insert(0, "{\"Data\":");
            jsonPolicyString = jsonPolicyString.Insert(jsonPolicyString.Length, "}");
            string procNameRole = "SECREF.pkg_webapi_POLICIES_POST_V01.prc_POST_ap"; //policy only
            OracleDynamicParameters oracleDynamicParametersPolicy = base.SetDBExecutionParams(jsonPolicyString, userId, "GTMData.AddPolicyWithRole.Post.v01");
            try
            {
                var result1 = await unitOfWork.TemplatesRepository.GenericDBCallAsync(
                    oracleDynamicParametersPolicy, procNameRole);

                if (oracleDynamicParametersPolicy.ParameterNames.Contains("pv_wl_id_o"))
                    entityId = oracleDynamicParametersPolicy.Get<string>("pv_wl_id_o");
            }

            catch (OracleException ex)
            {
                _logger.LogCritical($"Error saving policy to the database... {ex.Message}");
                return (
                       BadRequest($"An unknown error has occurred in saving your policy key")
                      );

            }
            // Note: if (result1.ReturnCode < 0) policy already exists, but should move forward to add role policy combination

            PolicyRolesResource policyRolesRes = new PolicyRolesResource
            {
                APKEY = policyRoles.APKEY,
                RLCD = policyRoles.RLCD,
            };

            var optionPolicyRoleRes = new JsonSerializerOptions
            {
                Converters =
                {
                    new BasePropertyNameConverter<PolicyRolesResource>()
                }
            };

            /******************************************************************************************************************/
            //Part 2- Create record for Policy role combo
            //******************************************************************************************************************
            string procNameRolePolicy = "SECREF.pkg_webapi_POLICIES_POST_V01.prc_POST_apr"; //policy and role

            //Logic for Policy insert first
            string jsonPolicyRolesString = JsonSerializer.Serialize(policyRolesRes, optionPolicyRoleRes).Insert(0, "{\"Data\":");
            jsonPolicyRolesString = jsonPolicyRolesString.Insert(jsonPolicyRolesString.Length, "}");

            //Input params
            OracleDynamicParameters oracleDynamicParameters = base.SetDBExecutionParams(jsonPolicyRolesString, userId, "GTMData.AddPolicyWithRole.Post.v01");

            var result = await unitOfWork.TemplatesRepository.GenericDBCallAsync(
                oracleDynamicParameters, procNameRolePolicy);

            if (oracleDynamicParameters.ParameterNames.Contains("pv_wl_id_o"))
                entityId = oracleDynamicParameters.Get<string>("pv_wl_id_o");

            if (result.ReturnCode < 0)
                errorId = result.ReturnCode;


            if (errorId < 0)
                return (
                    BadRequest($"An unknown error has occurred in saving your policy role record with error code from DB: {errorId}")
                    );
            return result switch
            {
                CommonOutcomes.InvalidData outcome => BadRequest(outcome.ParameterName),
                CommonOutcomes.NotFound _ => NotFound(),
                CommonOutcomes.UnauthorizedAccess _ => Unauthorized(),
                CommonOutcomes.Success<IEnumerable<dynamic>> response => Ok(JsonSerializer.Serialize(response, typeof(object), new JsonSerializerOptions() { WriteIndented = true })),
                _ => InternalServerError()
            };

        }



        /// <summary>
        /// This is an endpoint to the type ahead Resouce box  This fills the options for the "Select resources" typeahead.
        /// </summary>
        /// <returns>Returns JSON of result set of ResourceRoleAccessRepository GetAllAsyncResourceNames</retur
        [MapToApiVersion("1.0")]
        [Route("SecurityRoles/")]
        //[Authorize(Policy = Policies.IsAllUserRoles)]
        // [NonAction]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> SecurityRoles()
        {
            GenericRequestParameter parameters = new GenericRequestParameter();
            string[] dbArray = new string[] { _ver1.DbProcName, _ver1.CursorName };
            string[] orderBy = new string[] { "RCARLCD" };
            List<string> roleFilter = new List<string>(); ;
            Outcome result = null;

            try
            {
                parameters.resources = new string[] { _ver1.ResourceName };
                parameters.pageNum = 0;
                parameters.pageRows = 0;

                if (parameters.columns == null) //no columns sent from caller
                {
                    parameters.columns = new string[] { "RCARLCD", "distinct" };
                }

                if (!string.IsNullOrEmpty(_currentUser.AdAccountName))
                    parameters.adId = _currentUser.AdAccountName;
                else
                    parameters.adId = _ver1.ADId; //use token

                if (_currentUser.Roles != null)
                {
                    if (_currentUser.Roles.Any())
                    {
                        parameters.roles = _currentUser.Roles;
                    }
                }

                if (orderBy.Length > 0)
                {
                    parameters.orderBy = orderBy;
                }

                string[] apiVersion1 = new string[] { _ver1.DbProcName, _ver1.CursorName }; //api version 2

 

                parameters.filter = roleFilter.ToArray();
                result = await unitOfWork.DataShaping.DataOutcomeWithOutSecurityTrimmingAsync<dynamic>
                        (parameters, Request.GetDisplayUrl(), _ver1.EndPoint, apiVersion1);


                securityRoles pRoles = new securityRoles 
                {

                    Data = ((IEnumerable<dynamic>)result.Data)
                    .Select(x => new userRole
                    {
                        roleCode = x.RCARLCD
                    }).DistinctBy(x => x.roleCode)// Access RSRCNAME from each dynamic object
                    .ToList()
                };


                if (result is CommonOutcomes.NotFound)
                {
                    string requestedColumns = string.Join(",", parameters.columns);
                }

                return result switch
                {
                    CommonOutcomes.InvalidData outcome => BadRequest(outcome.ParameterName),
                    CommonOutcomes.NotFound _ => NotFound(),
                    CommonOutcomes.UnauthorizedAccess _ => Unauthorized(),
                    CommonOutcomes.Success<IEnumerable<dynamic>> response => Ok(pRoles),
                    _ => InternalServerError()
                };

            }
            catch (Exception ex)
            {

                return BadRequest(ex.InnerException);
            }
        }


        //******************************************************* Deprecated Passed testing *******************************************************
        /// <summary>
        /// This is an endpoint to the Column Resources page for the GTM Admin Dashboard.  This fills the options for the "Select Role" dropdown.
        /// </summary>
        /// <returns>Returns JSON of result set of ResourceRoleAccessRepository GetAllAsyncPolicyRoles</returns>
        [HttpGet]
        [MapToApiVersion("1.0")]
        [Route("SecurityRolesx/")]
       // [Authorize(Policy = Policies.IsAllUserRoles)]
       // [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<securityRoles> SecurityRolesx()
        {
            string auditKey = string.Empty;
            List<string> auditKeys = new List<string>();

            auditKey = base.GenerateCacheKey(auditKeys.ToArray());
            List<string> postsfilters = new List<string>(); ;
            RequestParameter parameters = new RequestParameter();
            string[] dbArray = new string[] { _ver1.DbProcName, _ver1.CursorName };
            ResourceRoleAccessRepository iResourceRepo = new ResourceRoleAccessRepository(dbConnection);

            var policyRoles = await iResourceRepo.GetAllAsyncPolicyRoles(parameters); //get all policy roles

            var roles = policyRoles.Select(x => new userRole
            {
                roleCode = x.RL_CD
            }).DistinctBy(x => x.roleCode)
            .ToList();

            securityRoles pRoles = new securityRoles();
            pRoles.Data = roles.Distinct();

            return pRoles;

        }

        /// <summary>
        /// This is an endpoint to the type ahead Resouce box  This fills the options for the "Select resources" typeahead.
        /// </summary>
        /// <returns>Returns JSON of result set of ResourceRoleAccessRepository GetAllAsyncResourceNames</retur
        [MapToApiVersion("1.0")]
        [Route("ResourceNames/")]
        [Authorize(Policy = Policies.IsAllUserRoles)]
        // [NonAction]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> ResourcesNames(string? roleCode)
        {
            GenericRequestParameter parameters = new GenericRequestParameter();
            string[] dbArray = new string[] { _ver1.DbProcName, _ver1.CursorName };
            string[] orderBy = new string[] { "RSRCName" };
            List<string> roleFilter = new List<string>(); ;
            Outcome result = null;

            try
            {
                parameters.resources = new string[] { _ver1.ResourceName };
                parameters.pageNum = 0;
                parameters.pageRows = 0;

                if (parameters.columns == null) //no columns sent from caller
                {
                    parameters.columns = new string[] { "RSRCNAME", "distinct" };
                }

                if (!string.IsNullOrEmpty(_currentUser.AdAccountName))
                    parameters.adId = _currentUser.AdAccountName;
                else
                    parameters.adId = _ver1.ADId; //use token

                if (_currentUser.Roles != null)
                {
                    if (_currentUser.Roles.Any())
                    {
                        parameters.roles = _currentUser.Roles;
                    }
                }

                if (orderBy.Length > 0)
                {
                    parameters.orderBy = orderBy;
                }

                string[] apiVersion1 = new string[] { _ver1.DbProcName, _ver1.CursorName }; //api version 2

                if (!string.IsNullOrEmpty(roleCode))
                    roleFilter.Add(string.Format($"RCARLCD" + "|{0}|{1}|", DBOperators.EQ, roleCode));

                parameters.filter = roleFilter.ToArray();
                result = await unitOfWork.DataShaping.DataOutcomeWithOutSecurityTrimmingAsync<dynamic>
                        (parameters, Request.GetDisplayUrl(), _ver1.EndPoint, apiVersion1);

                resourceNames rs = new resourceNames
                {
                    Data = ((IEnumerable<dynamic>)result.Data)
                    .Select(d => d.RSRCNAME) // Access RSRCNAME from each dynamic object
                    .ToArray()
                };

                if (result is CommonOutcomes.NotFound)
                {
                    string requestedColumns = string.Join(",", parameters.columns);
                }

                return result switch
                {
                    CommonOutcomes.InvalidData outcome => BadRequest(outcome.ParameterName),
                    CommonOutcomes.NotFound _ => NotFound(),
                    CommonOutcomes.UnauthorizedAccess _ => Unauthorized(),
                    CommonOutcomes.Success<IEnumerable<dynamic>> response => Ok(rs),
                    _ => InternalServerError()
                };

            }
            catch (Exception ex)
            {

                return BadRequest(ex.InnerException);
            }
        }

        /// <summary>
        /// This is an endpoint to the type ahead Resouce box  This fills the options for the "Select columns" typeahead.
        /// </summary>
        /// <returns>Returns JSON of result set of all columns</retur
        [MapToApiVersion("1.0")]
        [Route("AllColumns/")]
        [Authorize(Policy = Policies.IsAllUserRoles)]
        // [NonAction]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> ColumnsNames()
        {

            GenericRequestParameter parameters = new GenericRequestParameter();
            string[] dbArray = new string[] { _ver1.DbProcName, _ver1.CursorName };
            string[] orderBy = new string[] { "RCCOLUMNNAME" };
            List<string> roleFilter = new List<string>(); ;
            Outcome result = null;

            try
            {
                parameters.resources = new string[] { _ver1.ResourceName };
                parameters.pageNum = 0;
                parameters.pageRows = 0;

                if (parameters.columns == null) //no columns sent from caller
                {
                    parameters.columns = new string[] { "RCCOLUMNNAME", "distinct" };
                }

                if (!string.IsNullOrEmpty(_currentUser.AdAccountName))
                    parameters.adId = _currentUser.AdAccountName;
                else
                    parameters.adId = _ver1.ADId; //use token

                if (_currentUser.Roles != null)
                {
                    if (_currentUser.Roles.Any())
                    {
                        parameters.roles = _currentUser.Roles;
                    }
                }

                if (orderBy.Length > 0)
                {
                    parameters.orderBy = orderBy;
                }
                string[] apiVersion1 = new string[] { _ver1.DbProcName, _ver1.CursorName }; //api version 

                parameters.filter = roleFilter.ToArray();
                result = await unitOfWork.DataShaping.DataOutcomeWithOutSecurityTrimmingAsync<dynamic>
                        (parameters, Request.GetDisplayUrl(), _ver1.EndPoint, apiVersion1);

                columnNames rs = new columnNames
                {
                    Data = ((IEnumerable<dynamic>)result.Data)
                    .Select(d => d.RCCOLUMNNAME) // Access RSRCNAME from each dynamic object
                    .ToArray()
                };

                if (result is CommonOutcomes.NotFound)
                {
                    string requestedColumns = string.Join(",", parameters.columns);
                }


                return result switch
                {
                    CommonOutcomes.InvalidData outcome => BadRequest(outcome.ParameterName),
                    CommonOutcomes.NotFound _ => NotFound(),
                    CommonOutcomes.UnauthorizedAccess _ => Unauthorized(),
                    CommonOutcomes.Success<IEnumerable<dynamic>> response => Ok(rs),
                    _ => InternalServerError()
                };

            }
            catch (Exception ex)
            {

                return BadRequest(ex.InnerException);
            }
        }

        public static object GetPropertyValue(dynamic obj, string propertyName)
        {
            var propertyInfo = obj.GetType().GetProperty(propertyName);
            return propertyInfo?.GetValue(obj, null);
        }

        //******************************************************* Passed testing *******************************************************
        /// <summary>
        /// This is an endpoint to the Policy Roles page for the GTM Admin Dashboard
        /// </summary>
        /// <returns>Returns JSON of result set of ResourceRoleAccessRepository</returns>
        [HttpGet]
        [MapToApiVersion("1.0")]
        [Route("PolicyRoles/")]
        //[Authorize(Policy = Policies.IsAllUserRoles)]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<policyRoles> PolicyRoles()
        {
            string auditKey = string.Empty;
            List<string> auditKeys = new List<string>();

            auditKey = base.GenerateCacheKey(auditKeys.ToArray());
            List<string> postsfilters = new List<string>(); ;
            RequestParameter parameters = new RequestParameter();
            string[] dbArray = new string[] { _ver1.DbProcName, _ver1.CursorName };
            ResourceRoleAccessRepository iResourceRepo = new ResourceRoleAccessRepository(dbConnection);

            var roles = await iResourceRepo.GetAllAsyncPolicyRoles(parameters); //get all policy roles
            policyRoles pRoles = new policyRoles();
            pRoles.Data = roles;

            if (roles != null)
            {
                int roleCount = roles.Count();
                string allRoles = string.Join(",", roles);
            }
            else
            {
                // iLogger.LogCritical("Error Retrieving Roles from Database...");
            }

            return pRoles;

        }

        //******************************************************* (Deprecated) *******************************************************
        /// <summary>
        /// This is an endpoint to the Resource search for the GTM Admin Dashboard. 
        /// Note: This is being deprecated
        /// </summary>
        /// <returns>Returns JSON of result set of ResourceRoleAccessRepository</returns>
        [HttpGet]
        [MapToApiVersion("1.0")]
        [Route("ResourceColumns/")]
        [Authorize(Policy = Policies.IsAllUserRoles)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ColumnResources> ResourceColumns(string? roleCode, string resourceName, string? columnName)
        {
            string auditKey = string.Empty;

            List<string> auditKeys = new List<string>();
            auditKey = base.GenerateCacheKey(auditKeys.ToArray());

            RequestParameter parameters = new RequestParameter();
            string[] dbArray = new string[] { _ver1.DbProcName, _ver1.CursorName };

            ResourceColumnAccessRepository iResourceRepo = new ResourceColumnAccessRepository(dbConnection);
            var columns = await iResourceRepo.GetAllAsync(parameters);

            // Filter by role
            var filterColumns = roleCode == null ? columns : columns.Where(i => i.RL_CD == roleCode.Trim());

            // If resourceName is provided, apply additional filter
            if (!string.IsNullOrEmpty(resourceName))
            {
                filterColumns = filterColumns.Where(j => j.RSRC_NAME.ToUpper() == resourceName.Trim().ToUpper());
            }

            if (!string.IsNullOrEmpty(columnName))
            {
                filterColumns = filterColumns.Where(j => j.RC_COLUMN_NAME.ToUpper() == columnName.Trim().ToUpper());
            }

            // Apply ordering AFTER all filtering
            var sortedColumns = filterColumns
                .OrderBy(i => i.RL_CD)
                .ThenBy(i => i.RSRC_NAME)
                .ThenBy(i => i.RC_COLUMN_NAME);

            ColumnResources cR = new ColumnResources();
            cR.Data = sortedColumns; // assuming Data accepts IEnumerable<T>
            return cR;
        }
        //******************************************************************************************************************************



        //******************************************************* Passed testing *******************************************************
        /// <summary>
        /// Updated endpoint to the Resource search for the GTM Admin Dashboard
        /// </summary>
        /// <returns>Returns JSON of result set of Outcome.Data</retur
        [MapToApiVersion("2.0")]
        [Route("FilteredResources/")]
        [Authorize(Policy = Policies.IsAllUserRoles)]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> FilteredResources(string? roleCode, string resourceName, string? columnName)
        {
            GenericRequestParameter parameters = new GenericRequestParameter();
            string[] dbArray = new string[] { _ver1.DbProcName, _ver1.CursorName };
            List<string> resourceFilter = new List<string>(); ;
            Outcome result = null;

            try
            {
                parameters.resources = new string[] { _ver1.ResourceName };
                parameters.pageNum = 0;
                parameters.pageRows = 0;

                if (parameters.columns == null) //no columns sent from caller
                {
                    parameters.columns = new string[] { "RSRCNAME", "RCARLCD", "RCCOLUMNNAME", "distinct" };
                }

                if (!string.IsNullOrEmpty(_currentUser.AdAccountName))
                    parameters.adId = _currentUser.AdAccountName;
                else
                    parameters.adId = _ver1.ADId; //use token

                if (_currentUser.Roles != null)
                {
                    if (_currentUser.Roles.Any())
                    {
                        parameters.roles = _currentUser.Roles;
                    }
                }

                string[] apiVersion1 = new string[] { _ver1.DbProcName, _ver1.CursorName };

                if (!string.IsNullOrEmpty(roleCode))
                    resourceFilter.Add(string.Format($"RCARLCD" + "|{0}|{1}|", DBOperators.EQ, roleCode));

                if (!string.IsNullOrEmpty(resourceName))
                {
                    resourceFilter.Add(string.Format($"RSRCNAME" + "|{0}|{1}|", DBOperators.EQ, resourceName));
                }

                if (!string.IsNullOrEmpty(columnName))
                {
                    resourceFilter.Add(string.Format($"RCCOLUMNNAME" + "|{0}|{1}|", DBOperators.EQ, columnName));
                }

                parameters.filter = resourceFilter.ToArray();
                result = await unitOfWork.DataShaping.DataOutcomeWithOutSecurityTrimmingAsync<dynamic>
                        (parameters, Request.GetDisplayUrl(), _ver1.EndPoint, apiVersion1);

                resourceNames rs = new resourceNames
                {
                   Data = ((IEnumerable<dynamic>)result.Data)
                  .Select(d => new
                  {
                      resourceName = d.RSRCNAME,
                      columnName = d.RCCOLUMNNAME,
                      roleCode = d.RCARLCD,
                  })
                  .OrderBy(i => i.roleCode)
                  .ThenBy(i => i.resourceName)
                  .ThenBy(i => i.columnName)
                };

                return result switch
                {
                    CommonOutcomes.InvalidData outcome => BadRequest(outcome.ParameterName),
                    CommonOutcomes.NotFound _ => NotFound(),
                    CommonOutcomes.UnauthorizedAccess _ => Unauthorized(),
                    CommonOutcomes.Success<IEnumerable<dynamic>> response => Ok(rs),
                    _ => InternalServerError()
                };

            }
            catch (Exception ex)
            {

                return BadRequest(ex.InnerException);
            }
        }

        //******************************************************* (Deprecated) *******************************************************
        //Reason: IgnoreApi = true
        [HttpGet]
        [MapToApiVersion("1.0")]
        //[Route("Search/")]
        [Route("ResourceColumnsByResource/")]
        [Authorize(Policy = Policies.IsAllUserRoles)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ColumnResources> ResourceColumnsByResourceName(string resourceName)
        {
            string auditKey = string.Empty;
            List<string> auditKeys = new List<string>();
            auditKey = base.GenerateCacheKey(auditKeys.ToArray());
            List<string> postsfilters = new List<string>(); ;
            RequestParameter parameters = new RequestParameter();
            string[] dbArray = new string[] { _ver1.DbProcName, _ver1.CursorName };
            ResourceColumnAccessRepository iResourceRepo = new ResourceColumnAccessRepository(dbConnection);
            var columns = await iResourceRepo.GetAllAsync(parameters); //get all policy roles
            var filterColumns = columns.Where(i => i.RSRC_NAME == resourceName.ToUpper().Trim());
            ColumnResources cR = new ColumnResources();
            cR.Data = filterColumns;
            return cR;

        }

    }
}


