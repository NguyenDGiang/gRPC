using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using static GrpcService.Protos.UserService;
using GrpcService2.Data;
using GrpcService2.Models;
using GrpcService.Protos;
using System;
using Google.Protobuf.WellKnownTypes;

namespace GrpcService2Services
{
    public class UserService : UserServiceBase
    {
        public AppDbContext dbContext;
        public UserService(AppDbContext DBContext)
        {
            dbContext = DBContext;
        }

        public override Task<PagingUserResponse> GetPaging(PagingUserRequest request, ServerCallContext context)
        {
            var response = new PagingUserResponse();
            var count = dbContext.Users.Count();
            var pagingUser = dbContext.Users.Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize).Select(u => new UserProto()
                {
                    Id = u.Id,
                    Address = u.Address,
                    CreateDate = Timestamp.FromDateTime(DateTime.SpecifyKind(u.CreateDate, DateTimeKind.Utc)),
                    Delete = u.Delete,
                    Email = u.Email,
                    Name = u.Name,
                    Phone = u.Phone
                });

            response.Data.AddRange(pagingUser.ToArray());
            response.Count = count;
            response.PageIndex = request.PageIndex;
            response.PageSize = request.PageSize;
            return Task.FromResult(response);
        }

        public override Task<UserProtos> GetAll(EmptyProto request, ServerCallContext context)
        {
            UserProtos response = new UserProtos();
            var users = from u in dbContext.Users
                        where u.Delete == true
                        select new UserProto()
                        {
                            Id = u.Id,
                            Address = u.Address,
                            CreateDate = Timestamp.FromDateTime(DateTime.SpecifyKind(u.CreateDate, DateTimeKind.Utc)),
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
                CreateDate = Timestamp.FromDateTime(DateTime.SpecifyKind(res.Entity.CreateDate, DateTimeKind.Utc)),
                Delete = res.Entity.Delete,
                Email = res.Entity.Email,
                Name = res.Entity.Name,
                Phone = res.Entity.Phone
            };
            return Task.FromResult<UserProto>(response);
        }
        public override Task<UserProto> GetById(UserRowIdFilter request, ServerCallContext context)
        {
            
             var user = dbContext.Users.SingleOrDefault(x => x.Id == request.UserRowId);
            if(user == null)
            {

                throw new RpcException(new Status(StatusCode.NotFound, $"not find {request.UserRowId}"));
            }
            var userProto = new UserProto()
            {   
                Id = user.Id,
                Name = user.Name,
                Phone = user.Phone,
                Email = user.Email,
                Address = user.Address,
                Delete = user.Delete,
                CreateDate = Timestamp.FromDateTime(DateTime.SpecifyKind(user.CreateDate, DateTimeKind.Utc)),
                };
           
            
            return Task.FromResult(userProto);
        }

        public override Task<EmptyProto> Delete(UserRowIdFilter request, ServerCallContext context)
        {
            var userId = dbContext.Users.SingleOrDefault(x => x.Id == request.UserRowId);
            if(userId == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"not find {request.UserRowId}"));
            userId.Delete = false;
            dbContext.Users.Update(userId);
            dbContext.SaveChanges();
            return Task.FromResult(new EmptyProto());
        }

        public override Task<UserProto> Put(UserProto request , ServerCallContext context)
        {
            var userId = dbContext.Users.SingleOrDefault(x => x.Id == request.Id);
            if (userId == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"not find {request.Id}"));
            userId.Address = request.Address;
            userId.Delete = request.Delete;
            userId.Email = request.Email;
            userId.CreateDate = request.CreateDate.ToDateTime();
            userId.Name = request.Name;
            userId.Phone = request.Phone;
            dbContext.Users.Update(userId);
            dbContext.SaveChanges();
            return Task.FromResult(request);
        }
    }
}
