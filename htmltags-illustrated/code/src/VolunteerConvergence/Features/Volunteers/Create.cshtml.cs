using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Validators;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VolunteerConvergence.Models;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace VolunteerConvergence.Features.Volunteers
{
    public class CreateModel : PageModel
    {
        private readonly IMediator _mediator;

        [BindProperty]
        public Command Data { get; set; }


        public CreateModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _mediator.Send(Data);

            return this.RedirectToPageJson("Index");
        }

        public class Command : IRequest<Guid>
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string Email { get; set; }

            public DateTime? JoinDate { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.FirstName).NotEmpty().MaximumLength(500);
                RuleFor(c => c.LastName).NotEmpty().MaximumLength(500);
                RuleFor(c => c.Email).NotEmpty().EmailAddress(EmailValidationMode.AspNetCoreCompatible).MaximumLength(500);
                RuleFor(c => c.JoinDate).NotEmpty().InclusiveBetween(DateTime.Now.Date.AddYears(-20), DateTime.Now.Date);
            }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile() => CreateMap<Command, Volunteer>();
        }

        public class CommandHandler : IRequestHandler<Command, Guid>
        {
            private readonly VolConContext _db;
            private readonly IMapper _mapper;

            public CommandHandler(VolConContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Guid> Handle(Command request, CancellationToken cancellationToken)
            {
                var existingEmail = await _db.Volunteers.AnyAsync(v =>
                    v.Email == request.Email, cancellationToken);

                if (existingEmail)
                    throw new ValidationException("Email address already in use");

                var volunteer = _mapper.Map<Volunteer>(request);
                await _db.Volunteers.AddAsync(volunteer, cancellationToken);

                return volunteer.Id;
            }
        }
    }
}
