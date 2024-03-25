<template>
    <div id="app">
        <input v-model="number1" type="number" placeholder="Number 1" />
        <input v-model="number2" type="number" placeholder="Number 2" />
        <button @click="sumNumbers">Sum1</button>
        <div>Result: {{ result }}</div>
    </div>
</template>

<script>
export default {
    data() {
        return {
            number1: 0,
            number2: 0,
            result: 0,
        };
    },
    methods: {
        sumNumbers() {
            this.result = parseInt(this.number1) + parseInt(this.number2);
        },
    },
};
</script>