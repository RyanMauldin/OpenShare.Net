# OpenShare.Net
**OpenShare.Net** is a shared library with common components that I have created and adapted. I get some solutions and best practices from stackoverflow at times as well as other sources and I try to give credit where it is due. I plan to create a documentation site. Reach out to me if needed in the mean time. There are plenty of extension methods in Library.Common.

For Entity Framework stuff, the Library.Repository project has EntityIdentityRepository repository class and other variations that can be used. See Library.Repository.Repositories.EntityIdentityRepositories.cs for examples of extending and/or creating custom primary keys, etc. Note the Unit of Work pattern is there as an example, however it is tightly coupled to Entity Framework. I personally have used this Unit of Work for ease of passing it around in ASP.NET controllers via dependency injection with Unity and the PerRequestLifetimeManager. It was good for me in that scenario, where all I was dealing with was Entity Framework, and a Code First approach. The Commit method in Library.Repository.BaseUnitOfWork class will purge your cache for you. This is so fresh data can be pulled when returning back to the view in the MVC paradigm. This is built this way because by default, Entity Framework caches anything it can get it's hands on if AsNoTracking() is not used.

ConcurrentCache in Library.Threading project is a new in-memory cache system that is generically typed to IDictionary and is thread safe. ClientPool in Library.Threading project works well in a Task Parallel Library (TPL) scenario for any disposable clients. Note: I do unit testing seperately and this is not part of my build. I did seperate out the implementations in seperate NuGet packages so if you do not need the repositories for instance, that doesn't have to be included.

The following **NuGet** libraries are available:
* <https://www.nuget.org/packages/OpenShare.Net.Library.Common/>
* <https://www.nuget.org/packages/OpenShare.Net.Library.Repository/>
* <https://www.nuget.org/packages/OpenShare.Net.Library.Services/>
* <https://www.nuget.org/packages/OpenShare.Net.Library.Threading/>

**TFS Build Badge for Visual Studio Online**

![](https://ryanmauldin.visualstudio.com/_apis/public/build/definitions/1b6b2d4e-b829-47fc-92ef-e2e179a7005b/1/badge)

**Code Samples:**

*Library.Services.HttpService ...*

```
// This is just a sample, put in real endpoints yourself...
public async Task<string> TestHttpGetToApiAsync()
{
	var apiKey = "555555555555555555555555";
	var vin = "1A1AA1A11A1111111";
	var url = $"https://api.yoursite.com/api/vehicle/vins/{vin}?fmt=json&api_key={apiKey}";
	var httpService = new HttpService();
	return await httpService.RequestJsonAsync(HttpMethod.Get, url);
}
```