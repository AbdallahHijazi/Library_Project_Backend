using LibraryProject;
using LibraryProjectDomain.DTOS.BookDTO;
using LibraryProjectDomain.DTOS.MemberDTO;
using LibraryProjectDomain.Models.MembersModel;
using LibraryProjectRepository.Repositories.Members;
using LibraryProjectRepository.SheardRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProjectAPI.Controllers.MemberC
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly LibraryDbContext context;
        private readonly IRepository<Member> irepository;
        private readonly MemberRepository repository;

        public MemberController(LibraryDbContext context,
                                IRepository<Member> irepository,
                                MemberRepository repository)
        {
            this.context = context;
            this.irepository = irepository;
            this.repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMembersWithBorrowings()
        {
            var members = await repository.GetAllMembersWithBorrowings();
            if (members == null)
                return NotFound($"No member found in data base");
            return Ok(members);
        }

        [HttpGet("{id}",Name ="GetMember")]
        public async Task<IActionResult> GetMemberWithBorrowings(Guid id)
        {
            var member = await repository.GetMemberWithBorrowings(id);
            if (member == null)
                return NotFound($"No member found with id {id}");

            return Ok(member);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMember(MemberOpartion create)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var newMember = await repository.AddMember(create);

            return CreatedAtAction(nameof(GetMemberWithBorrowings),new { id = newMember.Id }, newMember);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMember(Guid id, MemberOpartion update)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var member = await repository.UpdateMember(id, update);
            if (member == null)
                return NotFound("This member cannot be modified because it does not exist");

            return Ok(member);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(Guid id)
        {
            try
            {
                var member = await repository.DeleteMember(id);
                if (!member)
                    return NotFound("Member not found or already deleted");
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            return NoContent();
        }
    }
}
