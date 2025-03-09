using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DesignPatternsCookbook.DomainDriven.Repository
{
    // ドメインエンティティ
    public class Customer
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public bool IsActive { get; private set; }

        public Customer(string name, string email)
        {
            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            IsActive = true;
        }

        // エンティティのビジネスロジック
        public void Deactivate()
        {
            IsActive = false;
        }

        public void UpdateEmail(string newEmail)
        {
            if (string.IsNullOrWhiteSpace(newEmail))
                throw new ArgumentException("Email cannot be empty");

            Email = newEmail;
        }
    }

    // リポジトリインターフェース
    public interface ICustomerRepository
    {
        Task<Customer> GetByIdAsync(Guid id);
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<IEnumerable<Customer>> GetActiveCustomersAsync();
        Task AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task<bool> DeleteAsync(Guid id);
    }

    // インメモリ実装（テスト用）
    public class InMemoryCustomerRepository : ICustomerRepository
    {
        private readonly List<Customer> _customers = new List<Customer>();

        public Task<Customer> GetByIdAsync(Guid id)
        {
            return Task.FromResult(_customers.FirstOrDefault(c => c.Id == id));
        }

        public Task<IEnumerable<Customer>> GetAllAsync()
        {
            return Task.FromResult(_customers.AsEnumerable());
        }

        public Task<IEnumerable<Customer>> GetActiveCustomersAsync()
        {
            return Task.FromResult(_customers.Where(c => c.IsActive));
        }

        public Task AddAsync(Customer customer)
        {
            _customers.Add(customer);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Customer customer)
        {
            var index = _customers.FindIndex(c => c.Id == customer.Id);
            if (index != -1)
            {
                _customers[index] = customer;
            }
            return Task.CompletedTask;
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            var index = _customers.FindIndex(c => c.Id == id);
            if (index != -1)
            {
                _customers.RemoveAt(index);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }

    // Entity Frameworkを使用した実装（概念的なコード）
    public class EfCustomerRepository : ICustomerRepository
    {
        private readonly DbContext _context;

        public EfCustomerRepository(DbContext context)
        {
            _context = context;
        }

        public async Task<Customer> GetByIdAsync(Guid id)
        {
            return await _context.Set<Customer>().FindAsync(id);
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Set<Customer>().ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
        {
            return await _context.Set<Customer>()
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        public async Task AddAsync(Customer customer)
        {
            await _context.Set<Customer>().AddAsync(customer);
        }

        public Task UpdateAsync(Customer customer)
        {
            _context.Set<Customer>().Update(customer);
            return Task.CompletedTask;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var customer = await GetByIdAsync(id);
            if (customer == null)
                return false;

            _context.Set<Customer>().Remove(customer);
            return true;
        }
    }
    
    // 使用例
    public class CustomerService
    {
        private readonly ICustomerRepository _repository;
        
        public CustomerService(ICustomerRepository repository)
        {
            _repository = repository;
        }
        
        public async Task<Customer> RegisterCustomerAsync(string name, string email)
        {
            var customer = new Customer(name, email);
            await _repository.AddAsync(customer);
            return customer;
        }
        
        public async Task<bool> DeactivateCustomerAsync(Guid id)
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null)
                return false;
                
            customer.Deactivate();
            await _repository.UpdateAsync(customer);
            return true;
        }
        
        public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
        {
            return await _repository.GetActiveCustomersAsync();
        }
    }
    
    // 注：DbContextとその関連クラスはここでは実装していません
    // 実際のコードではEntity Frameworkの参照が必要です
    public class DbContext
    {
        public virtual DbSet<T> Set<T>() where T : class => null;
    }
    
    public class DbSet<T> where T : class
    {
        public virtual Task<T> FindAsync(object id) => null;
        public virtual Task<List<T>> ToListAsync() => null;
        public virtual IQueryable<T> Where(Func<T, bool> predicate) => null;
        public virtual Task AddAsync(T entity) => Task.CompletedTask;
        public virtual void Update(T entity) { }
        public virtual void Remove(T entity) { }
    }
}