using CRM.AlMohtaref.Application.Common.Exceptions;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Common;

public class ExceptionsTests
{
    [Fact] public void BadRequest_Is400() => Assert.Equal(400, new BadRequestException("m").StatusCode);

    [Fact] public void Unauthorized_Is401() => Assert.Equal(401, new UnauthorizedException("m").StatusCode);

    [Fact] public void Forbidden_Is403() => Assert.Equal(403, new ForbiddenException("m").StatusCode);

    [Fact] public void NotFound_Is404() => Assert.Equal(404, new NotFoundException("m").StatusCode);

    [Fact] public void Conflict_Is409() => Assert.Equal(409, new ConflictException("m").StatusCode);


    [Fact]
    public void Exceptions_KeepTheirMessage()
    {
        Assert.Equal("boom", new NotFoundException("boom").Message);
        Assert.Equal("boom", new ConflictException("boom").Message);
    }
}
