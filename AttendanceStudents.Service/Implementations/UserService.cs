using AttendanceStudents.Repository;
using AttendanceStudents.Service.Interfaces;
using AttendanceStudents.Domain.Entities;

namespace AttendanceStudents.Service.Implementations;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;

    public UserService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public User? Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        return _userRepository.Get(u => u.Username == username && u.Password == password);
    }
}