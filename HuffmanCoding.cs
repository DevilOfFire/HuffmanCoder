// Created by Beniamin Gajecki

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace HuffmanCoder
{
    delegate void TreeVisitor<T>(Tree<T> node); // Funkcja lambda(linq) dla działania na objektach
    delegate void TreeDataVisitor<T>(T nodeData); // Funkcja lambda(linq) dla działania na dacie
    class Tree<T> // Klasa drzewa
    {
        public Tree(T data)
        {
            this.data = data;
            this.parent = null;
            this.children = new LinkedList<Tree<T>>();
        }
        private Tree(T data, Tree<T> parent)
        {
            this.data = data;
            this.parent = parent;
            this.children = new LinkedList<Tree<T>>();
        }

        public void AddChild(T data)
        {
            this.children.AddFirst(new Tree<T>(data, this));
        }

        public void AddChild(Tree<T> child)
        {
            child.parent = this;
            this.children.AddFirst(child);
        }

        public void SetData(T data)
        {
            this.data = data;
        }

        public T GetData()
        {
            return this.data;
        }

        public Tree<T> GetParent()
        {
            return this.parent;
        }

        public Tree<T> GetChild(int i)
        {
            foreach (Tree<T> child in children)
                if (--i == 0)
                    return child;
            return null;
        }

        public LinkedList<Tree<T>> GetChildren()
        {
            return this.children;
        }

        public void TraverseData(Tree<T> node, TreeDataVisitor<T> visitor) // Data traverse
        {
            visitor(node.data);
            foreach (Tree<T> child in node.children)
                TraverseData(child, visitor);

        }

        public void Traverse(Tree<T> node, TreeVisitor<T> visitor) // Tree traverse
        {
            visitor(node);
            foreach (Tree<T> child in node.children)
                Traverse(child, visitor);

        }

        private T data;
        private Tree<T> parent;
        private LinkedList<Tree<T>> children;
    }

    struct Data_t // Struktura zawierająca symbol i prawdopodobieństwo(potrzebna do drzewa Huffmana)
    {
        public byte symbol;
        public uint probability;
        public bool site;
        public bool isLeaf;
        public Data_t(byte symbol) // Konstruktor dla liści
        {
            this.symbol = symbol;
            this.probability = 1;
            this.site = false;
            this.isLeaf = true;
        }
        public Data_t(uint probability) // Konstruktor dla węzłów
        {
            this.symbol = 0;
            this.probability = probability;
            this.site = false;
            this.isLeaf = false;
        }
    }

    [Serializable]
    struct HuffmanElement_t // Struktura zawierająca symbol i kod(potrzebna do tabeli Huffmana)
    {
        public byte symbol;
        public string uniqueCode;
    }

    class HuffmanTree
    {
        public HuffmanTree(List<Data_t> data) // Stworzenie drzewa Huffmana
        {
            List<Tree<Data_t>> nTree = new List<Tree<Data_t>>(); // Stworzenie drzew binarnych, gdzie węzły przechowują symbol i prawdopodobieństwo
            foreach (Data_t n in data)
                nTree.Add(new Tree<Data_t>(n));

            if (nTree.Count == 1) // Jeżeli data składa się tylko z jednego znaku
            {
                Tree<Data_t> leaf = nTree.First();
                nTree.RemoveAt(0);

                Data_t leafData = leaf.GetData();
                leafData.site = true;
                leaf.SetData(leafData);

                Data_t newData = new Data_t(1u);
                Tree<Data_t> newTree = new Tree<Data_t>(newData);
                newTree.AddChild(leaf);

                tree = newTree;
            }
            else
            {
                nTree.Sort((first, second) => first.GetData().probability.CompareTo(second.GetData().probability)); // Sortowanie drzew po prawdopodobieństwie
                // Dopóki na liście jest więcej niż jedno drzewo usuwaj dwa drzewa z najmniejszym prawdopodobieństwem i twórz z nich jedno większe drzewo
                while (nTree.Count > 1)
                {
                    Tree<Data_t> left = nTree.First(); // Pierwsze drzewo na liście
                    nTree.RemoveAt(0);
                    Tree<Data_t> right = nTree.First(); // Drugie drzewo na liście przed usunięciem pierwszego
                    nTree.RemoveAt(0);


                    Data_t rightData = right.GetData();
                    rightData.site = true; // Zmiana strony
                    right.SetData(rightData);

                    Data_t newData = new Data_t(left.GetData().probability + right.GetData().probability);
                    Tree<Data_t> newTree = new Tree<Data_t>(newData);
                    newTree.AddChild(left);
                    newTree.AddChild(right);

                    bool isAdded = false;
                    for (int i = 0; i < nTree.Count; ++i)
                    {
                        if (nTree[i].GetData().probability >= newTree.GetData().probability)
                        {
                            nTree.Insert(i, newTree);
                            isAdded = true;
                            break;
                        }
                    }
                    if (!isAdded)
                    {
                        nTree.Add(newTree);
                    }
                }
                tree = nTree.First();
            }
        }

        public Tree<Data_t>[] GetLeafsFromTree() // Stwórz listę zawierającą liście drzewa
        {
            List<Tree<Data_t>> Leafs = new List<Tree<Data_t>>();
            tree.Traverse(tree, (obj) =>
            {
                if (obj.GetData().isLeaf == true)
                    Leafs.Add(obj);
            });
            return Leafs.ToArray();
        }

        public string GeneratePath(Tree<Data_t> node) // Tworzenie unikalnego kodu po przez przebycie drogi do korzenia
        {
            string path = "";
            while (node.GetParent() != null)
            {
                if (node.GetData().site)
                    path = "1" + path;
                else
                    path = "0" + path;
                node = node.GetParent();
            }
            return path;
        }

        public HuffmanElement_t[] GetHuffmanTable() // Pobierz dane z drzewa i zamień w tabele Huffmana
        {
            List<HuffmanElement_t> HuffmanTable = new List<HuffmanElement_t>();
            Tree<Data_t>[] Leafs = GetLeafsFromTree();

            foreach (Tree<Data_t> leaf in Leafs)
            {
                HuffmanElement_t element;
                element.symbol = leaf.GetData().symbol;
                element.uniqueCode = GeneratePath(leaf);
                HuffmanTable.Add(element);
            }
            HuffmanTable.Sort((first, second) => second.uniqueCode.Length.CompareTo(first.uniqueCode.Length));
            return HuffmanTable.ToArray();
        }

        private Tree<Data_t> tree; // Drzewo Huffmana
    }

    class Program
    {
        public static void CodeFile(byte[] data, string filename)
        {
            HuffmanElement_t[] hTab = CreateHuffmanTable(data);

            string binaryData = ""; // Tworzenie daty w systemie binarnym przy użyciu kodowania Huffmana
            foreach (byte i in data)
            {
                foreach (HuffmanElement_t element in hTab)
                {
                    if (i == element.symbol)
                    {
                        binaryData += element.uniqueCode;
                        break;
                    }
                }
            }

            byte[] newData = new byte[(int)Math.Ceiling((float)binaryData.Length / 8.0)]; // Stworzenie nowej daty, która jest przekonwertowaniem binaryData

            for (int i = 0, j = 0; i < binaryData.Length; i += 8, ++j)
            {
                if (binaryData.Length - i < 8)
                    newData[j] = Convert.ToByte(binaryData.Substring(i), 2);
                else
                    newData[j] = Convert.ToByte(binaryData.Substring(i, 8), 2);
            }

            var stream = new FileStream(filename + ".hTab", FileMode.Create); // Rozszerzenie pliku .hTab od nazwy struktury (HuffmanTable_t)
            var bFormatter = new BinaryFormatter();
            bFormatter.Serialize(stream, hTab); // Zapisywanie tabeli Huffmana
            stream.Close();

            File.WriteAllBytes(filename + ".hComp", newData); // Rozszerzenie pliku .hComp od Huffman Compresion
            System.Windows.MessageBox.Show("Plik został zakodowany pomyślnie!");
        }

        public static void DecodeFile(byte[] data, string filename)
        {
            if (Path.GetExtension(filename) == ".hComp")
            {
                string hTabPath = Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename) + ".hTab";
                if (File.Exists(hTabPath))
                {
                    var stream = new FileStream(hTabPath, FileMode.Open);
                    var bformatter = new BinaryFormatter();
                    HuffmanElement_t[] hTab = (HuffmanElement_t[])bformatter.Deserialize(stream); // Odczytywanie tabeli Huffmana
                    stream.Close();

                    string binaryData = "";
                    string str = "";

                    for (int i = 0; i < data.Length; ++i)
                    {
                        str = Convert.ToString(data[i], 2);

                        if (data.Length - 1 != i) // Ostatni bit bez wypełnienia
                            while (str.Length < 8)
                                str = "0" + str;

                        binaryData += str;
                        str = "";
                    }

                    List<byte> newData = new List<byte>();

                    for (int i = 0; i < binaryData.Length;)
                    {
                        for (int j = 0; j < hTab.Length; ++j)
                        {
                            int codeLenght = hTab[j].uniqueCode.Length;
                            /*
                            if (i + codeLenght < binaryData.Length)
                                code = binaryData.Substring(i, codeLenght);
                            else
                                break;
                            */

                            for (int k = 0; k < codeLenght; ++k) // Szybsze od substring 
                            {
                                if (i + k < binaryData.Length)
                                    str += binaryData[i + k];
                                else
                                    break;
                            }
                            if (str == hTab[j].uniqueCode)
                            {
                                newData.Add(hTab[j].symbol);
                                i += codeLenght;
                                break;
                            }
                            str = "";
                        }
                        
                    }

                    File.WriteAllBytes(Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename), newData.ToArray());
                    System.Windows.MessageBox.Show("Plik został zdekodowany pomyślnie!");
                }
                else
                    System.Windows.MessageBox.Show("Brak pliku " + Path.GetFileNameWithoutExtension(filename) + ".hTab!");
            }
            else
                System.Windows.MessageBox.Show("Błędny format pliku!");
        }

        private static HuffmanElement_t[] CreateHuffmanTable(byte[] data)
        {
            List<Data_t> character = new List<Data_t>();
            for (UInt64 i = 0; i < (UInt64)data.LongLength; ++i) // Wrazie bardzo długich plików wykorzystuje unsigned long
            {
                bool isExist = false;
                for (int j = 0; j < character.Count; ++j)
                {
                    if (data[i] == character[j].symbol)
                    {
                        isExist = true; // Element już wystąpił wcześniej

                        Data_t element = character[j]; // Zwiększam liczbe wystąpień
                        element.probability++;
                        character[j] = element;
                    }
                }
                if (isExist == false) // Element wcześniej nie występował
                    character.Add(new Data_t(data[i]));
            }

            HuffmanTree hTree = new HuffmanTree(character);
            HuffmanElement_t[] hTab = hTree.GetHuffmanTable();

            return hTab;
        }
    }
}
