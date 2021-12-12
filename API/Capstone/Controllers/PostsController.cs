﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Capstone.Models;
using Capstone.DAO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Capstone.Services;

namespace Capstone.Controllers
{
    [Route("[controller]")]
    [ApiController]
    
    public class PostsController : ControllerBase
    {
        private readonly IPostDao postDao;
        private readonly IFavoritePostDao favoritePostDao;
        private readonly ILikePostDao likePostDao;
        private IFileStorageService fileStorageService;

        public PostsController(IPostDao _postDao, IFavoritePostDao _favoritePostDao, ILikePostDao _likePostDao)
        {
            postDao = _postDao;
            favoritePostDao = _favoritePostDao;
            likePostDao = _likePostDao;
            fileStorageService = new AWSS3FileStorage();
        }

        [HttpGet("/posts")]
        public ActionResult<List<Post>> GetPosts()
        {
            return Ok(postDao.GetAllPosts());

        }

        [HttpGet("/posts/{postId}")] 
        [AllowAnonymous]
        public Post GetPost(int postId)
        {
            Post post = postDao.GetPost(postId);
            if (post.PostId != postId)
            {
                return null;
            } 
            else
            {
                return post;
            }
            
        }
        [HttpPost("/posts")]
        public IActionResult UploadPost([FromForm] NewUploadPost newUploadPost)
        {

            //(Post post, IFormFile uploadImg)
            Post post = new Post
            {
                AccountId = newUploadPost.AccountId,
                Caption = newUploadPost.Caption,
                Timestamp = newUploadPost.Timestamp
            };

            string mediaLink = fileStorageService.UploadFileToStorage(newUploadPost.uploadImg);
            post.MediaLink = mediaLink;
            Post createdPost = postDao.UploadPost(post);
            if (createdPost != null)
            {
                 return Created($"/{post.PostId}", createdPost);
            }
            return BadRequest(new { message = "Could not process your post." });
        }
        [HttpPut("/posts/{postId}")]
        public ActionResult<Post> UpdatePost(Post updatedPost, int postId)
        {
            bool result = true;
            Post existingPost = postDao.GetPost(postId);
            if(existingPost != null)
            {
                updatedPost.AccountId = existingPost.AccountId;
                updatedPost.PostId = existingPost.PostId;

                result = postDao.UpdatePost(postId, updatedPost.MediaLink);
            } 
            else if (existingPost == null)
            {
                return NotFound();
            }
            if(result)
            {
                return Ok();
            } 
            else
            {
                return StatusCode(500);
            }
        }

        [HttpGet("/posts/{postId}/like")]
        public ActionResult<List<int>> GetAccountsWhoLikedPost(int postId)
        {
            List<int> idList = likePostDao.GetAccountIdsLikingPost(postId);

            return Ok(idList);
        }

        [HttpPost("/posts/{postId}/like")] //WORK IN PROGRESS
        public IActionResult LikePost(LikePost likePost)
        {
            //IActionResult result = BadRequest(new { message = "Could not like this post." });
            bool newLikedPost = likePostDao.LikePost(likePost);
            if (newLikedPost == true)
            {
                return Ok();
            } 
            else
            {
                return BadRequest();
            }
        }

        [HttpDelete("/posts/{postId}/like")]

        public IActionResult RemoveLikedPost(LikePost likePost)
        {
            //IActionResult result = BadRequest(new { message = "Could not remove the post." });
            bool deletedPost = likePostDao.UnlikePost(likePost);
            if (deletedPost == true)
            {
                return NoContent();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet("/posts/favorites/{accountId}")]

        

        public List<Post> GetFavoritePosts(int accountId)
        {
            List<Post> listpost = favoritePostDao.GetListOfFavoritePosts(accountId);
            if (listpost.Count == 0)
            {
                return null;
            } 
            else
            {
                return listpost;
            }
        }

        [HttpPost("/posts/favorites")]
        public ActionResult AddFavoritePost(FavoritePost favoritePost)
        {
            bool newFavoritePost = favoritePostDao.AddFavoritePost(favoritePost);
            if (newFavoritePost == true)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpDelete("/posts/favorites/{accountId}")]
        public ActionResult RemoveFavoritePost(FavoritePost favoritePost)
        {
            bool removedPost = favoritePostDao.RemoveFavoritePost(favoritePost);
            if(removedPost == true)
            {
                return NoContent();
            } 
            else
            {
                return BadRequest();
            }
        }

    }

}