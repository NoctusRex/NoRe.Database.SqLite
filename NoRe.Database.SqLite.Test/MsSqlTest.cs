using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoRe.Core;
using NoRe.Database.Core.Models;

namespace NoRe.Database.SqLite.Test
{
    [TestClass]
    public class MsSqlTest
    {
        private readonly string Path = System.IO.Path.Combine(Pathmanager.StartupDirectory, "test.db");

        [TestMethod]
        public void TestConfiguration()
        {
            SqLiteWrapper wrapper = null;

            try
            {
                try { wrapper = new SqLiteWrapper(); Assert.Fail(); } catch { }
                try { wrapper = new SqLiteWrapper(Path, "3"); } catch (Exception ex) { Assert.IsTrue(ex.Message.Contains("Unable to connect")); }
                try { wrapper = new SqLiteWrapper(); Assert.Fail(); } catch { }

                _ = new SqLiteWrapper(Path, "3", "", true);
                Assert.IsTrue(File.Exists(System.IO.Path.Combine(Pathmanager.ConfigurationDirectory, "SqLiteConfiguration.xml")));

                _ = new SqLiteWrapper();

            }
            finally
            {
                if (wrapper != null) wrapper.Dispose();
                DeleteConfiguration();
            }
        }

        [TestMethod()]
        public void TestNonQuery()
        {
            using (SqLiteWrapper wrapper = new SqLiteWrapper(Path, "3"))
            {
                int key = new Random().Next(3, 9999999);
                Assert.IsTrue(wrapper.ExecuteNonQuery($"INSERT INTO test (id, value) VALUES (@0, @1)", key, "test") == 1);
                Assert.IsTrue(wrapper.ExecuteNonQuery($"DELETE FROM test WHERE id = {key};") == 1);
            }
        }

        [TestMethod()]
        public void TestQuery()
        {
            using (SqLiteWrapper wrapper = new SqLiteWrapper(Path, "3"))
            {
                Table t = wrapper.ExecuteReader("SELECT * FROM test");

                Assert.AreEqual(1, t.GetValue<Int64>(0, "id"));
                Assert.AreEqual("already exists", t.GetValue<string>(0, "value"));
                Assert.AreEqual(2, t.GetValue<Int64>(1, "id"));
                Assert.AreEqual("Hello World", t.GetValue<string>(1, "value"));
            }
        }

        [TestMethod()]
        public void TestScalar()
        {
            using (SqLiteWrapper wrapper = new SqLiteWrapper(Path, "3"))
            {
                Assert.AreEqual("Hello World", wrapper.ExecuteScalar<string>("SELECT value FROM test WHERE id = @0", 2));
            }
        }

        [TestMethod()]
        public void TestTransaction1()
        {
            using (SqLiteWrapper wrapper = new SqLiteWrapper(Path, "3"))
            {
                Table t1 = wrapper.ExecuteReader("SELECT * FROM test");

                try
                {
                    List<Query> queries = new List<Query>
                    {
                        new Query("INSERT INTO test (id, value) VALUES (45618, 'test')"),
                        new Query("INSERT INTO test (id, value) VALUES (1, 'test')")
                    };

                    wrapper.ExecuteTransaction(queries);
                    Assert.Fail();
                }
                catch { }

                Assert.IsTrue(CompareTables(t1, wrapper.ExecuteReader("SELECT * FROM test")));
            }
        }

        [TestMethod()]
        public void TestTransaction2()
        {
            using (SqLiteWrapper wrapper = new SqLiteWrapper(Path, "3"))
            {
                Table t1 = wrapper.ExecuteReader("SELECT * FROM test");

                try
                {
                    List<Query> queries = new List<Query>
                    {
                        new Query("INSERT INTO test (id, value) VALUES (45618, 'test')"),
                        new Query("INSERT INTO test (id, value) VALUES (3, 'test')")
                    };

                    wrapper.ExecuteTransaction(queries);
                    Assert.Fail();
                }
                catch { }

                Assert.IsFalse(CompareTables(t1, wrapper.ExecuteReader("SELECT * FROM test")));

                wrapper.ExecuteTransaction("DELETE FROM test WHERE id = @0 OR id = @1", 45618, 3);

                Assert.IsTrue(CompareTables(t1, wrapper.ExecuteReader("SELECT * FROM test")));
            }
        }

        private bool CompareTables(Table t1, Table t2)
        {
            if (t1.Rows.Count != t2.Rows.Count) return false;

            for (int i = 0; i < t1.Rows.Count; i++)
            {
                for (int c = 0; c < t1.DataTable.Columns.Count; c++)
                {
                    if (!Equals(t1.DataTable.Rows[i][c], t2.DataTable.Rows[i][c]))
                        return false;
                }
            }
            return true;
        }

        private void DeleteConfiguration()
        {
            try
            {
                Directory.Delete(Pathmanager.ConfigurationDirectory, true);
            }
            catch { }
        }
    }
}
