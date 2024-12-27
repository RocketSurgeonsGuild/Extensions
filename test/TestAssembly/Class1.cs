namespace TestAssembly;

public interface IService;

public interface IServiceB;

public class Service : IService, IServiceB;

public static class Nested
{
    public class ServiceA : IService;

    public class GenericServiceA : IGenericService<string>, IOther;

    private record MyRecord;

    private class Validator : IValidator<MyRecord>;
}

class ServiceB : IService;

public interface IRequest<T>;

public interface IRequestHandler<T, R> where T : IRequest<R>;

class Request : IRequest<Response>;

class Response;

class RequestHandler : IRequestHandler<Request, Response>;

public interface IOther;

public interface IGenericService<T>;

class GenericService : IGenericService<int>, IGenericService<string>, IOther;

public class GenericServiceB : IGenericService<decimal>, IOther;

public interface IValidator;

public interface IValidator<T> : IValidator;
