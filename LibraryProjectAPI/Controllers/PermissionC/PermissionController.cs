using LibraryProjectDomain.DTOS.PermissionDTO;
using LibraryProjectDomain.Models.PermissionModel;
using LibraryProjectRepository.Repositories.Permissions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProjectAPI.Controllers.PermissionC
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly PermissionRepository repository;

        public PermissionController(PermissionRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllPermissions()
        {
            var permissions = await repository.GetAllPermissions();
            if (permissions == null)
                return NotFound("not found permision in database");
            return Ok(permissions);
        }
        [HttpGet("{id}",Name ="GetPermission")]
        public async Task<ActionResult> GetPermissionById(Guid id)
        {
            var permission =await repository.GetPermissionById(id);
            if (permission == null)
                return NotFound($"Not found permision have id:{id} in database ");
            return Ok(permission);
        }
        [HttpPost]
        public async Task<ActionResult> CreatePermission(PermissionOparation create)
        {
            var permission = await repository.CreatePermission(create);
            if (permission == null)
                return NotFound("The input value is error please try again");
            return CreatedAtAction(nameof(GetPermissionById), new { id = permission.Id }, permission);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatePermission(Guid id, PermissionOparation update)
        {
            var permission = await repository.UpdatePermission(id,update);
            if (permission == null)
                return NotFound($"Not found permision have id:{id} in database for update");
            return Ok(permission);
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePermission(Guid id)
        {
            var permission = await repository.DeletePermission(id);
            if (permission == null)
                return NotFound("This permission has already been deleted or does not exist.");
            return NoContent();
        }
    }
}
