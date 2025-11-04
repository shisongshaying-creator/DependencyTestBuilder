using System;
using System.Collections.Generic;
using System.Linq;

namespace SampleCode
{
    /// <summary>
    /// ユーザーを管理するクラス
    /// </summary>
    public class UserManager
    {
        private List<User> users = new List<User>();

        /// <summary>
        /// 新しいユーザーを追加します
        /// </summary>
        /// <param name="user">追加するユーザー</param>
        /// <returns>追加が成功した場合はtrue</returns>
        public bool AddUser(User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Name))
            {
                return false;
            }

            users.Add(user);
            Console.WriteLine($"ユーザー '{user.Name}' を追加しました。");
            return true;
        }

        /// <summary>
        /// IDでユーザーを検索します
        /// </summary>
        public User GetUserById(int id)
        {
            return users.FirstOrDefault(u => u.Id == id);
        }

        /// <summary>
        /// すべてのユーザーを取得します
        /// </summary>
        public List<User> GetAllUsers()
        {
            return new List<User>(users);
        }

        /// <summary>
        /// ユーザーを削除します
        /// </summary>
        public bool DeleteUser(int id)
        {
            var user = GetUserById(id);
            if (user != null)
            {
                users.Remove(user);
                Console.WriteLine($"ユーザー '{user.Name}' を削除しました。");
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// ユーザー情報を表すクラス
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }

        public User(int id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
            CreatedAt = DateTime.Now;
        }

        public override string ToString()
        {
            return $"User[ID={Id}, Name={Name}, Email={Email}]";
        }
    }
}
