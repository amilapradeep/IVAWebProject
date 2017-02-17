﻿using IVA.DbAccess;
using IVA.DbAccess.Repository;
using IVA.DTO;
using IVA.DTO.Contract;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IVA.FindExpert.Controllers
{
    public class AroundMeController : BaseController
    {
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetServiceCategories()
        {
            List<IServiceCategory> categories = null;
            try
            {
                using (AppDBContext context = new AppDBContext())
                {
                    categories = new ServiceCategoryRepository(context).GetAllRoot();
                }
            }
            catch(Exception ex)
            {
                return InternalServerError(ex);
            }
            return Ok(categories);
        }

        [HttpGet]
        [Authorize]
        public IHttpActionResult GetServiceProviders(int CategoryId, double Longitude, double Latitude)
        {
            List<ServiceLocation> providers = null;
            try
            {
                using (AppDBContext context = new AppDBContext())
                {
                    var categoryId = new SqlParameter("CategoryId", CategoryId);
                    var longitude = new SqlParameter("@Longitude", Longitude);
                    var latitude = new SqlParameter("@Latitude", Latitude);
                    providers = context.Database.SqlQuery<ServiceLocation>(
                        "uspGetNearestServiceProviders @CategoryId, @Longitude, @Latitude ", categoryId, longitude, latitude).ToList();
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
            return Ok(providers);
        }
    }
}