﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Capstone.DAO;
using Capstone.Models;

namespace Capstone.DAO
{
    public class PostSqlDao : IPostDao
    {
        private readonly string connectionString;
        public PostSqlDao(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }
        public Post GetPost(int postId)
        {
            Post returnPost = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT post_id, account_id, media_link, caption, timestamp FROM posts WHERE post_id = @post_id", conn);
                    cmd.Parameters.AddWithValue("@post_id", postId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        returnPost = GetPostFromReader(reader);
                    }
                }
                return returnPost;
            }
            catch
            {
                throw;
            }
        }
        public void UploadPost(Post post)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("INSERT INTO posts (account_id, media_link, caption, timestamp) " + 
                        "output inserted.post_id " + 
                        "VALUES(@account_id, @media_link, @caption, @timestamp)", conn);
                    cmd.Parameters.AddWithValue("@account_id", post.AccountId);
                    cmd.Parameters.AddWithValue("@media_link", post.MediaLink);
                    cmd.Parameters.AddWithValue("@caption", post.Caption);
                    cmd.Parameters.AddWithValue("@timestamp", post.Timestamp);

                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                throw;
            }
        }

        private Post GetPostFromReader(SqlDataReader reader)
        {
            Post post = new Post()
            {
                PostId = Convert.ToInt32(reader["post_id"]),
                AccountId = Convert.ToInt32(reader["account_id"]),
                MediaLink = Convert.ToString(reader["media_link"]),
                Caption = Convert.ToString(reader["caption"]),
                Timestamp = Convert.ToDateTime(reader["timestamp"])
            };
            return post;
        }

       
    }
}