using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureBank.Helpers.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequiresClaimAttribute : Attribute, IAuthorizationFilter
    {
        #region FIELDS

        private readonly string _claimName;
        private readonly string _claimValue;

        #endregion



        #region CONSTRUCTORS

        public RequiresClaimAttribute(string claimName, string claimValue)
        {
            _claimName = claimName;
            _claimValue = claimValue;
        }

        #endregion



        #region PUBLIC METHODS

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.HasClaim(_claimName, _claimValue))
            {
                context.Result = new ForbidResult();
            }
        }

        #endregion
    }
}
