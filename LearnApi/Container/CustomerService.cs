using AutoMapper;
using LearnApi.Helper;
using LearnApi.Model;
using LearnApi.Repos;
using LearnApi.Repos.Models;
using LearnApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LearnApi.Container
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerService> _logger;
        public CustomerService(ApplicationDbContext context, IMapper mapper, ILogger<CustomerService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<APIResponse> Create(CustomerModel data)
        {
            APIResponse response = new APIResponse();
            try
            {
                Student std = _mapper.Map<CustomerModel, Student>(data);
                await _context.Students.AddAsync(std);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Create Student : "+data.Id);
                response.ResponseCode = 201;
                response.Result = !string.IsNullOrEmpty(data.RollNumber) ? data.RollNumber.ToString() : "0";
            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.ErrorMessage = ex.Message;
                _logger.LogError("Create Student "+ex.Message);
            }
            return response;
        }

        public async Task<List<CustomerModel>> GetAll()
        {
            _logger.LogInformation("Get All Students");
            List<CustomerModel> response = new List<CustomerModel>();
            var data = await _context.Students.ToListAsync();
            if (data != null && data.Count > 0)
            {
                response = _mapper.Map<List<Student>, List<CustomerModel>>(data);
            }
            return response;
        }

        public async Task<CustomerModel> GetStudentById(int id)
        {
            CustomerModel response = new CustomerModel();
            var data = await _context.Students.FindAsync(id);
            if (data != null)
            {
                _logger.LogInformation("Get Student : "+id);
                response = _mapper.Map<Student, CustomerModel>(data);
            }
            return response;
        }

        public async Task<APIResponse> Remove(int id)
        {
            APIResponse response = new APIResponse();
            try
            {
                var std = await _context.Students.FindAsync(id);
                if (std != null)
                {
                    _context.Students.Remove(std);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Remove Student : "+id);
                    response.ResponseCode = 200;
                    response.Result = id.ToString();

                }
                else
                {
                    response.ResponseCode = 404;
                    response.Result = "data not found";
                }
            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<APIResponse> Update(CustomerModel data, int id)
        {
            APIResponse response = new APIResponse();
            try
            {
                var std = await _context.Students.FindAsync(id);
                if (std != null)
                {
                    std.Name = data.Name;
                    std.Age = data.Age;
                    std.RollNumber = data.RollNumber;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Update Student : "+id);
                    response.ResponseCode = 200;
                    response.Result = id.ToString();
                }
                else
                {
                    response.ResponseCode = 404;
                    response.Result = "data not found";
                }
            }
            catch (Exception ex)
            {
                response.ResponseCode = 400;
                response.ErrorMessage = ex.Message;
            }
            return response;
        }
    }
}
