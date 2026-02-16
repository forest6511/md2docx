#!/usr/bin/env node
/**
 * Test suite for skill-activation-prompt.js
 *
 * Run: node test_skill_activation.js
 */

const assert = require('assert');
const path = require('path');

// Load the module (update path to actual location)
const skillActivationPath = path.join(__dirname, '../../.claude/hooks/skill-activation-prompt.js.refactored');
const { analyzeAndActivateSkills, calculateConfidence } = require(skillActivationPath);

// Mock skill rules
const mockSkillRules = {
  skills: [
    {
      name: 'adr-source-validator',
      triggers: {
        keywords: ['adr', 'decision', 'architecture'],
        paths: ['docs/decisions/**'],
        confidence_threshold: 0.7
      }
    },
    {
      name: 'doc-source-sync',
      triggers: {
        keywords: ['documentation', 'readme', 'sync'],
        paths: ['README.md', 'docs/**'],
        confidence_threshold: 0.6
      }
    }
  ]
};

// Test suite
function runTests() {
  console.log('🧪 Running skill activation tests...\n');

  let passed = 0;
  let failed = 0;

  // Test 1: Keyword matching
  {
    const prompt = 'validate adr decisions';
    const context = { workingFiles: [], inGitRepo: true };
    const results = analyzeAndActivateSkills(prompt, context, mockSkillRules);

    const adrSkill = results.find(s => s.name === 'adr-source-validator');

    if (adrSkill && adrSkill.confidence >= 0.6) {
      console.log('✅ Test 1 passed: Keyword matching activates correct skill');
      passed++;
    } else {
      console.log('❌ Test 1 failed: Expected adr-source-validator to activate');
      failed++;
    }
  }

  // Test 2: Path matching
  {
    const prompt = 'check this file';
    const context = {
      workingFiles: ['docs/decisions/ADR-0001.md'],
      inGitRepo: true
    };
    const results = analyzeAndActivateSkills(prompt, context, mockSkillRules);

    const adrSkill = results.find(s => s.name === 'adr-source-validator');

    if (adrSkill && adrSkill.confidence > 0) {
      console.log('✅ Test 2 passed: Path matching activates correct skill');
      passed++;
    } else {
      console.log('❌ Test 2 failed: Expected adr-source-validator for ADR file');
      failed++;
    }
  }

  // Test 3: Confidence threshold filtering
  {
    const prompt = 'random unrelated task';
    const context = { workingFiles: [], inGitRepo: true };
    const results = analyzeAndActivateSkills(prompt, context, mockSkillRules);

    if (results.length === 0) {
      console.log('✅ Test 3 passed: Low confidence skills filtered out');
      passed++;
    } else {
      console.log('❌ Test 3 failed: Expected no skills to activate');
      console.log('   Activated:', results.map(s => s.name));
      failed++;
    }
  }

  // Test 4: Multiple skills activation
  {
    const prompt = 'sync documentation and validate adr';
    const context = {
      workingFiles: ['README.md', 'docs/decisions/ADR-0001.md'],
      inGitRepo: true
    };
    const results = analyzeAndActivateSkills(prompt, context, mockSkillRules);

    if (results.length >= 2) {
      console.log('✅ Test 4 passed: Multiple skills can activate');
      passed++;
    } else {
      console.log('❌ Test 4 failed: Expected at least 2 skills to activate');
      failed++;
    }
  }

  // Test 5: Synonym matching
  {
    const prompt = 'check architecture decision records';
    const context = { workingFiles: [], inGitRepo: true };
    const results = analyzeAndActivateSkills(prompt, context, mockSkillRules);

    const adrSkill = results.find(s => s.name === 'adr-source-validator');

    if (adrSkill) {
      console.log('✅ Test 5 passed: Synonym matching works');
      passed++;
    } else {
      console.log('❌ Test 5 failed: Expected synonym "architecture decision" to match');
      failed++;
    }
  }

  // Summary
  console.log('\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
  console.log(`Test Results: ${passed} passed, ${failed} failed`);
  console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');

  process.exit(failed > 0 ? 1 : 0);
}

// Run tests
runTests();
