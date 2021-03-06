using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Capstone.Models;
using System.Data.SqlClient;

namespace Capstone.DAO
{
    public class CommentSqlDao : ICommentDao
    {
        private readonly string ConnectionString;
        public CommentSqlDao(string connString)
        {
            ConnectionString = connString;
        }

        public Comment CreateComment(Comment comment)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("INSERT INTO comments (account_id, post_id, timestamp, text) " +
                    "output inserted.comment_id " +
                    "VALUES(@account_id, @post_id, @timestamp, @text)", conn);
                    cmd.Parameters.AddWithValue("@account_id", comment.AccountId);
                    cmd.Parameters.AddWithValue("@post_id", comment.PostId);
                    cmd.Parameters.AddWithValue("@timestamp", comment.TimeStamp);
                    cmd.Parameters.AddWithValue("@text", comment.Text);

                    comment.CommentId = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            catch (SqlException)
            {
                throw;
            }
            return comment;
        }

        public List<Comment> GetCommentsByPost(int postId)
        {
            List<Comment> PostComments = new List<Comment>();
            

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    string sqlText = "SELECT comments.comment_id, comments.account_id, comments.post_id, comments.timestamp, comments.text, users.username " +
                        "FROM comments JOIN accounts ON comments.account_id = accounts.account_id JOIN users ON accounts.user_id = users.user_id " +
                        "WHERE comments.post_id = @post_id;";

                    SqlCommand cmd = new SqlCommand(sqlText, connection);

                    cmd.Parameters.AddWithValue("@post_id", postId);

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Comment tempComment = GetCommentFromReader(reader);
                        PostComments.Add(tempComment);                        

                    }
                }
            }
            catch (SqlException)
            {

                throw;
            }
            return PostComments;
        }
        private Comment GetCommentFromReader(SqlDataReader reader)
        {
            Comment comment = new Comment()
            {
                AccountId = Convert.ToInt32(reader["account_id"]),
                CommentId = Convert.ToInt32(reader["comment_id"]),
                PostId = Convert.ToInt32(reader["post_id"]),
                TimeStamp = Convert.ToDateTime(reader["timestamp"]),
                Text = Convert.ToString(reader["text"]),
                Username = Convert.ToString(reader["username"])
            };
            return comment;
        }
    }
}



