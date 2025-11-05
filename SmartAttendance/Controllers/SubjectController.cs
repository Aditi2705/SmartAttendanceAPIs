using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.DTOs;
using SmartAttendance.DTOs.Admin;
using SmartAttendance.Interfaces;
using SmartAttendance.Models;
using SmartAttendance.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartAttendance.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectRepository _subjectRepository;
        private readonly IMapper _mapper;

        public SubjectController(ISubjectRepository subjectRepository, IMapper mapper)
        {
            _subjectRepository = subjectRepository;
            _mapper = mapper;
        }

        // ✅ 1️⃣ Get all subjects
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var subjects = await _subjectRepository.GetAllAsync();
            var result = _mapper.Map<IEnumerable<GetSubjectDto>>(subjects);
            return Ok(result);
        }

        // ✅ 2️⃣ Get subject by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var subject = await _subjectRepository.GetByIdAsync(id);
            if (subject == null)
                return NotFound();

            var result = _mapper.Map<GetSubjectDto>(subject);
            return Ok(result);
        }

        // ✅ 3️⃣ Create subject
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSubjectDto dto)
        {
            var subject = _mapper.Map<Subject>(dto);
            await _subjectRepository.AddAsync(subject);
            return Ok(_mapper.Map<GetSubjectDto>(subject));
        }

        // ✅ 4️⃣ Update subject
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSubjectDto dto)
        {
            var subject = await _subjectRepository.GetByIdAsync(id);
            if (subject == null)
                return NotFound();

            _mapper.Map(dto, subject);
            await _subjectRepository.UpdateAsync(subject);
            return Ok(_mapper.Map<GetSubjectDto>(subject));
        }

        // ✅ 5️⃣ Delete subject
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _subjectRepository.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
