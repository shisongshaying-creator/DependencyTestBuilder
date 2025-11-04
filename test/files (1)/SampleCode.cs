using System;
using System.Collections.Generic;

namespace SampleApp
{
    /// <summary>
    /// ユーザー情報を管理するクラス
    /// </summary>
    public class UserManager
    {
        private List<User> users = new List<User>();
        
        /// <summary>
        /// 新しいユーザーを追加します
        /// </summary>
        public void AddUser(string name, int age)
        {
            var user = new User
            {
                Id = users.Count + 1,
                Name = name,
                Age = age,
                CreatedAt = DateTime.Now
            };
            users.Add(user);
            Console.WriteLine($"ユーザー追加: {user.Name} (ID: {user.Id})");
        }
        
        /// <summary>
        /// すべてのユーザーを表示します
        /// </summary>
        public void DisplayAllUsers()
        {
            Console.WriteLine("\n=== ユーザー一覧 ===");
            foreach (var user in users)
            {
                Console.WriteLine($"ID: {user.Id}, 名前: {user.Name}, 年齢: {user.Age}");
            }
        }
        
        /// <summary>
        /// IDでユーザーを検索します
        /// </summary>
        public User? FindUserById(int id)
        {
            return users.Find(u => u.Id == id);
        }
    }
    
    /// <summary>
    /// ユーザー情報を表すクラス
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
