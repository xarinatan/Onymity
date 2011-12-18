#include <stdio.h>
#include <iostream>
#include <string>
#include <list>

using namespace std;

int main(int argc, char** args)
{

}

class Node  
{
private:
	string privtext;
	list<Node*> privchildnodes;
public:
	Node(char* _text)
	{
		privtext = _text;
	}
	Node(char* _text, Node* _childnode)
	{
		privtext = _text;
		privchildnodes.push_back(_childnode);
	}
	string text
	{
		get{ return privtext;}
		set{privtex = value;}
	}
};