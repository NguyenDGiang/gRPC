using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using static GrpcService.Protos.UserService;
using GrpcService2.Data;
using GrpcService2.Models;
using GrpcService.Protos;
using System;

namespace GrpcService2Services
{
    public class UserService : UserServiceBase
    {
        public AppDbContext dbContext;
        public UserService(AppDbContext DBContext)
        {
            dbContext = DBContext;
        }

        public override Task<UserProtos> GetAll(Empty request, ServerCallContext context)
        {
            UserProtos response = new UserProtos();
            var users = from u in dbContext.Users
                        select new UserProto()
                        {
                            Id = u.Id,
                            Address = u.Address,
                            CreateDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(u.CreateDate),
                            Delete = u.Delete,
                            Email = u.Email,
                            Name = u.Name,
                            Phone = u.Phone
                        };
            response.Items.AddRange(users.ToArray());
            return Task.FromResult(response);
        }
        public override Task<UserProto> Post(UserProto request, ServerCallContext context)
        {

            var prdAdded = new User
            {
                Address = request.Address,
                CreateDate = request.CreateDate.ToDateTime(),
                Delete = request.Delete,
                Email = request.Email,
                Name = request.Name,
                Phone = request.Phone
            };
            var res = dbContext.Users.Add(prdAdded);
            dbContext.SaveChanges();
            var response = new UserProto()
            {
                Address = res.Entity.Address,
                CreateDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(res.Entity.CreateDate),
                Delete = res.Entity.Delete,
                Email = res.Entity.Email,
                Name = res.Entity.Name,
                Phone = res.Entity.Phone
            };
            return Task.FromResult<UserProto>(response);
        }
    }
}
