using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedMango_API.Data;
using RedMango_API.Models;
using RedMango_API.Models.Dto;
using RedMango_API.Services;
using RedMango_API.Utility;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

namespace RedMango_API.Controllers
{
	[Route("api/MenuItem")]
	[ApiController]
	public class MenuItemController : ControllerBase
	{
		private readonly ApplicationDbContext _db;
		private readonly IBlobService _blobService;
		private ApiResponse _response;
		public MenuItemController(ApplicationDbContext db, IBlobService blobService)
		{
			_db = db;
			_response = new ApiResponse();
			_blobService = blobService;
		}

		[HttpGet]
		public async Task<IActionResult> GetMenuItems()
		{
			_response.Result = _db.MenuItems;
			_response.StatusCode = HttpStatusCode.OK;
			return Ok(_response);
		}

		[HttpGet("{id:int}", Name = "GetMenuItem")]
		public async Task<IActionResult> GetMenuItem(int id)
		{
			if (id == 0)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				return BadRequest(_response);
			}
			MenuItem menuItem = _db.MenuItems.FirstOrDefault(x => x.Id == id);
			if (menuItem == null)
			{
				_response.StatusCode = HttpStatusCode.NotFound;
				_response.IsSuccess = false;
				return NotFound(_response);
			}
			_response.Result = menuItem;
			_response.StatusCode = HttpStatusCode.OK;
			return Ok(_response);
		}

		[HttpPost]
		public async Task<ActionResult<ApiResponse>> CreateMenuItem([FromForm] MenuItemCreateDTO menuItemCreateDTO) //We are using [FromForm] because we want to upload an image!
		{
			try
			{
				if (ModelState.IsValid)
				{
					if (menuItemCreateDTO.File == null || menuItemCreateDTO.File.Length == 0)
					{
						_response.StatusCode = HttpStatusCode.BadRequest;
						_response.IsSuccess = false;
						return BadRequest();
					}
					string fileName = $"{Guid.NewGuid()}{Path.GetExtension(menuItemCreateDTO.File.FileName)}";  //It will get us a new file name
					MenuItem menuItemToCreate = new()
					{
						Name = menuItemCreateDTO.Name,
						Price = menuItemCreateDTO.Price,
						Category = menuItemCreateDTO.Category,
						Description = menuItemCreateDTO.Description,
						SpecialTag = menuItemCreateDTO.SpecialTag,
					    Image = menuItemCreateDTO.File.ToString(),
					};
					//if (menuItemCreateDTO.File != null)
					//{
					//	string extension = Path.GetExtension(menuItemCreateDTO.File.FileName).ToLower();  //It will gives us the FilemName of type (IFormFile)
					//	if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
					//	{
					//		byte[] buffer = new byte[menuItemCreateDTO.File.Length];
					//		menuItemCreateDTO.File.OpenReadStream().Read(buffer, 0, buffer.Length);
					//		menuItemToCreate.Image = buffer.ToString();

					//	}
					//}

					_db.MenuItems.Add(menuItemToCreate);
					_db.SaveChanges();
					_response.Result = menuItemToCreate;
					_response.StatusCode = HttpStatusCode.Created;
					return CreatedAtRoute("GetMenuItem", new { id = menuItemToCreate.Id }, _response);
				}

				else
				{
					_response.IsSuccess = false;
					return NotFound();
				}
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessages = new List<string>() { ex.ToString()

			};
			return _response;
			}

		}
		[HttpPut("{id:int}")]
		public async Task<ActionResult<ApiResponse>> UpdateMenuItem(int id, [FromForm] MenuItemUpdateDTO menuItemUpdateDTO)
		{
			try
			{
				if (ModelState.IsValid)
				{
					if (menuItemUpdateDTO == null || id != menuItemUpdateDTO.Id)
					{
						_response.StatusCode = HttpStatusCode.BadRequest;
						_response.IsSuccess = false;
						return BadRequest();
					}

					MenuItem menuItemFromDB = await _db.MenuItems.FindAsync(id); //it will only search for the primary key on the table which is the "Id" column

					if (menuItemFromDB == null)
					{
						_response.StatusCode = HttpStatusCode.BadRequest;
						_response.IsSuccess = false;
						return BadRequest();
					}

					menuItemFromDB.Name = menuItemUpdateDTO.Name;
					menuItemFromDB.Price = menuItemUpdateDTO.Price;
					menuItemFromDB.Description = menuItemUpdateDTO.Description;
					menuItemFromDB.Category = menuItemUpdateDTO.Category;
					menuItemFromDB.SpecialTag = menuItemUpdateDTO.SpecialTag;
					//if (menuItemUpdateDTO.File != null && menuItemUpdateDTO.File.Length > 0)
					//{
					//	//string fileName = $"{Guid.NewGuid()}{Path.GetExtension(menuItemUpdateDTO.File.FileName)}";  //It will get us a new file name
					//	//await _blobService.DeleteBlob(menuItemFromDB.Image.Split('/').Last(), SD.SD_Storage_Container);  // It will delete the last image which we uploaded before and make that empty to replace the new one!
						menuItemFromDB.Image = menuItemUpdateDTO.File.ToString();  // After we deleted the last image before, Now we upload our new image
					//}
					//if (menuItemUpdateDTO.File != null)
					//{
					//	string extension = Path.GetExtension(menuItemUpdateDTO.File.FileName).ToLower();  //It will gives us the FilemName of type (IFormFile)
					//	if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
					//	{
					//		byte[] buffer = new byte[menuItemUpdateDTO.File.Length];
					//		menuItemUpdateDTO.File.OpenReadStream().Read(buffer, 0, buffer.Length);
					//		menuItemFromDB.Image = buffer.ToString();

					//	}
					//}

					_db.MenuItems.Update(menuItemFromDB);
					_db.SaveChanges();
					_response.StatusCode = HttpStatusCode.NoContent;
					return Ok(_response);
				}
				else
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					return NotFound();
				}
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessages = new List<string>() { ex.ToString() };
			}
			return _response;
		}

		[HttpDelete("{id:int}")]
		public async Task<ActionResult<ApiResponse>> DeleteMenuItem(int id)
		{
			try
			{

				if (id == 0)
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.IsSuccess = false;
					return BadRequest();
				}

				MenuItem menuItemFromDB = await _db.MenuItems.FindAsync(id); //it will only search for the primary key on the table which is the "Id" column

				if (menuItemFromDB == null)
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.IsSuccess = false;
					return BadRequest();
				}
				//await _blobService.DeleteBlob(menuItemFromDB.Image.Split('/').Last(), SD.SD_Storage_Container);

				int milliseconds = 2000;
				Thread.Sleep(milliseconds);  //Delay for 2 seconds

				_db.MenuItems.Remove(menuItemFromDB);
				_db.SaveChanges();
				_response.StatusCode = HttpStatusCode.NoContent;
				return Ok(_response);

			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorMessages = new List<string>() { ex.ToString() };
			}
			return _response;
		}
	}
}
