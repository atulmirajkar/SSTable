using System;
namespace AVLTree
{
    public class AVL{

        public class Node{
            public Node left { get; set; }
            public Node right{get; set;} 
            public int value{get; set;}
            
            public int height{get; set;}
            public int bf { get; set; }

            public Node(int value){
                this.value = value;
                height = 1;
            }
        }

        private Node root{get; set;}

        //todo implement size
        private int size{get; set;}

        private static void updateHeight(Node node){
            if(node == null)
                return;
            
            int leftHeight = node.left != null ? node.left.height : 0;
            int rightHeight = node.right != null ? node.right.height : 0;
            node.height = 1+ Math.Max(leftHeight,rightHeight);

            node.bf = leftHeight - rightHeight;   
        }

        private static Node rotateRight(Node node){
            if(node == null)
                return null;
            
            Node pivot = node.left;
            //if pivot itself is null dont do anything
            if(pivot == null)
                return node;

            Node temp = pivot.right;
            pivot.right = node;
            node.left = temp;
            
            //update heights
            //update input node's height first because it goes below the pivot node
            updateHeight(node);
            updateHeight(pivot);
            return pivot;
        }

        private static Node rotateLeft(Node node){
            if(node == null)
                return null;
            
            Node pivot = node.right;
            //if pivot itself is null dont do anything
            if(pivot == null)
                return node;
            
            Node temp = pivot.left;
            pivot.left = node;
            node.right = temp;
            

            //update heights
            updateHeight(node);
            updateHeight(pivot);

            return pivot;
        }

        private static Node rebalance(Node node, int value){
            if(node == null)
                return null;

            int balance = node.bf;
            //left - left case
            if(balance > 1){
                if(value < node.left.value)
                    return rotateRight(node);
                
                //left - right case
                if(value > node.left.value){
                    node.left = rotateLeft(node.left);
                    return rotateRight(node);
                }
            }else if(balance < 1){
                //right right case
                if(value > node.right.value)
                    return rotateLeft(node);

                //right left case
                if(value < node.right.value){
                    node.right = rotateRight(node.right);
                    return rotateLeft(node);
                }
            }
            return null;
        }
       

        private Node insert(int value, Node node){
            //exit condition
            if(node == null){
                return new Node(value);
            }

            //perform normal bst insertion
            //dont allow duplicates
            if(node.value == value){
                return node;
            }
            if(value < node.value)
            {
                node.left = insert(value, node.left);
            }else{
                node.right = insert(value, node.right);
            }

            //update height as we unwind the stack
            updateHeight(node);

            //check whether bf within limits
            if(node.bf < -1 ||  node.bf > 1){
                return rebalance(node, value);
            }
            
            return node;
        }
       
        private void preOrder(Node node){
            //exit condition 
            if(node == null)
                return;
            
            Console.WriteLine(node.value);
            preOrder(node.left);
            preOrder(node.right);
        }

        //public methods
        public void PreOrder()
        {
            preOrder(root);
        }

        public Node Insert(int value)
        {
            root = insert(value, root);
            return root;
        }
    }
}