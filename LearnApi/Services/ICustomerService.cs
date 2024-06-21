using LearnApi.Helper;
using LearnApi.Model;
using LearnApi.Repos;
using LearnApi.Repos.Models;
using System.Runtime.InteropServices;

namespace LearnApi.Services
{
    public interface ICustomerService
    {
        public Task<List<CustomerModel>> GetAll();
        public Task<CustomerModel> GetStudentById(int id);
        public Task<APIResponse> Remove(int id);
        public Task<APIResponse> Create(CustomerModel data);
        public Task<APIResponse> Update(CustomerModel data, int id);
    }
}
